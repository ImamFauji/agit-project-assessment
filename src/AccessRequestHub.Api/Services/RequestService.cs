using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Exceptions;
using AccessRequestHub.Api.Models.DTOs;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Models.Enums;
using AccessRequestHub.Api.Repositories.Interfaces;
using AccessRequestHub.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AccessRequestHub.Api.Services;
public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly AppDbContext _context;

    public RequestService(
        IRequestRepository requestRepo,
        IAuditRepository auditRepo,
        AppDbContext context)
    {
        _requestRepo = requestRepo;
        _auditRepo = auditRepo;
        _context = context;
    }
    public async Task<(AccessRequestResponse Response, bool WasCreated)> CreateRequestAsync(
        string requesterEmail,
        CreateRequestDto dto)
    {
        var requester = await GetUserOrThrowAsync(requesterEmail);
        if (requester.Role != UserRole.Requester)
            throw new ForbiddenException("Only a user with the Requester role can create an access request.");

        if (string.IsNullOrWhiteSpace(dto.ClientRequestId))
            throw new BadRequestException("ClientRequestId is required.");

        if (string.IsNullOrWhiteSpace(dto.Justification))
            throw new BadRequestException("Justification is required.");
        if (!Enum.IsDefined(dto.Environment) || !Enum.IsDefined(dto.AccessLevel))
            throw new BadRequestException("Environment and AccessLevel must contain valid values.");
        var existing = await _requestRepo.GetByClientRequestIdAsync(dto.ClientRequestId);
        if (existing is not null)
        {
            return (MapToResponse(existing), WasCreated: false);
        }
        var application = await _context.Applications.FindAsync(dto.ApplicationId)
            ?? throw new NotFoundException($"Application with ID '{dto.ApplicationId}' not found.");
        var newRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            ClientRequestId = dto.ClientRequestId,
            RequesterEmail = requester.Email,
            ApplicationId = dto.ApplicationId,
            Environment = dto.Environment,
            AccessLevel = dto.AccessLevel,
            Justification = dto.Justification,
            Status = RequestStatus.PendingManager,   // Business Rule: all requests start here
            PolicyVersion = "v1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _requestRepo.AddAsync(newRequest);
        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            RequestId = newRequest.Id,
            Timestamp = DateTime.UtcNow,
            ActorEmail = requester.Email,
            Action = "Created",
            OldStatus = null,                        // No previous status for creation event
            NewStatus = RequestStatus.PendingManager,
            Reason = null
        };

        await _auditRepo.AddAsync(auditEvent);
        try
        {
            // One SaveChanges call commits both the request and its first audit event.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            var duplicate = await _requestRepo.GetByClientRequestIdAsync(dto.ClientRequestId);
            if (duplicate is not null)
                return (MapToResponse(duplicate), WasCreated: false);

            throw;
        }
        newRequest.Application = application;

        return (MapToResponse(newRequest), WasCreated: true);
    }
    public async Task<IEnumerable<AccessRequestResponse>> ListRequestsAsync(string actorEmail)
    {
        var actor = await GetUserOrThrowAsync(actorEmail);

        IEnumerable<AccessRequest> requests = actor.Role switch
        {
            UserRole.Requester    => await _requestRepo.ListByRequesterAsync(actorEmail),
            UserRole.Manager      => await _requestRepo.ListPendingForManagerAsync(actorEmail),
            UserRole.SystemOwner  => await _requestRepo.ListPendingForSystemOwnerAsync(actorEmail),
            UserRole.Auditor      => await _requestRepo.ListAllAsync(),
            _                     => throw new ForbiddenException("Unknown user role.")
        };

        return requests.Select(MapToResponse);
    }
    public async Task<AccessRequestDetailResponse?> GetRequestDetailAsync(string actorEmail, Guid requestId)
    {
        var actor = await GetUserOrThrowAsync(actorEmail);
        var request = await _requestRepo.GetByIdAsync(requestId);
        if (request is null) return null;

        var mayRead = actor.Role == UserRole.Auditor
            || (actor.Role == UserRole.Requester && SameEmail(request.RequesterEmail, actor.Email))
            || (actor.Role == UserRole.Manager
                && await IsDirectManagerAsync(actor.Email, request.RequesterEmail))
            || (actor.Role == UserRole.SystemOwner
                && SameEmail(request.Application.OwnerEmail, actor.Email));

        if (!mayRead)
            throw new ForbiddenException("You are not allowed to view this request.");

        return MapToDetailResponse(request);
    }
    public async Task<AccessRequestResponse> ApproveRequestAsync(
        string actorEmail,
        Guid requestId,
        ApproveRequestDto dto)
    {
        var actor = await GetUserOrThrowAsync(actorEmail);
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException($"Request '{requestId}' not found.");
        if (request.Status is RequestStatus.Approved or RequestStatus.Rejected)
            throw new BadRequestException(
                $"Request is already in a terminal state ({request.Status}). No further transitions allowed.");
        if (request.RequesterEmail.Equals(actorEmail, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("You cannot approve your own request.");
        if (request.Version != dto.Version)
            throw new ConcurrencyException(
                "The request has been modified by another process. Please reload and try again.");
        string action;
        RequestStatus oldStatus = request.Status;
        RequestStatus newStatus;

        if (request.Status == RequestStatus.PendingManager)
        {
            var requester = await GetUserOrThrowAsync(request.RequesterEmail);
            if (actor.Role != UserRole.Manager || !SameEmail(requester.ManagerEmail, actor.Email))
                throw new ForbiddenException(
                    "Only the requester's direct manager can approve a request in PendingManager state.");
            bool isHighRisk = IsHighRisk(request);
            if (isHighRisk)
            {
                newStatus = RequestStatus.PendingSystemOwner;
                action = "ApprovedByManager";
            }
            else
            {
                newStatus = RequestStatus.Approved;
                action = "ApprovedByManager";
            }
        }
        else if (request.Status == RequestStatus.PendingSystemOwner)
        {
            if (actor.Role != UserRole.SystemOwner || !SameEmail(request.Application.OwnerEmail, actor.Email))
                throw new ForbiddenException(
                    "Only the application's System Owner can approve a request in PendingSystemOwner state.");

            newStatus = RequestStatus.Approved;
            action = "ApprovedBySystemOwner";
        }
        else
        {
            throw new BadRequestException($"Request cannot be approved from status '{request.Status}'.");
        }
        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;
        _requestRepo.Update(request);
        await _auditRepo.AddAsync(new AuditEvent
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Timestamp = DateTime.UtcNow,
            ActorEmail = actorEmail,
            Action = action,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = null
        });
        await SaveTransitionWithAuditAsync();

        return MapToResponse(request);
    }
    public async Task<AccessRequestResponse> RejectRequestAsync(
        string actorEmail,
        Guid requestId,
        RejectRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new BadRequestException("A rejection reason is required and cannot be empty.");

        var actor = await GetUserOrThrowAsync(actorEmail);
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException($"Request '{requestId}' not found.");
        if (request.Status is RequestStatus.Approved or RequestStatus.Rejected)
            throw new BadRequestException(
                $"Request is already in a terminal state ({request.Status}). No further transitions allowed.");
        if (request.RequesterEmail.Equals(actorEmail, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("You cannot reject your own request.");
        if (request.Version != dto.Version)
            throw new ConcurrencyException(
                "The request has been modified by another process. Please reload and try again.");
        string action;
        RequestStatus oldStatus = request.Status;

        if (request.Status == RequestStatus.PendingManager)
        {
            var requester = await GetUserOrThrowAsync(request.RequesterEmail);
            if (actor.Role != UserRole.Manager || !SameEmail(requester.ManagerEmail, actor.Email))
                throw new ForbiddenException(
                    "Only the requester's direct manager can reject a request in PendingManager state.");
            action = "RejectedByManager";
        }
        else if (request.Status == RequestStatus.PendingSystemOwner)
        {
            if (actor.Role != UserRole.SystemOwner || !SameEmail(request.Application.OwnerEmail, actor.Email))
                throw new ForbiddenException(
                    "Only the application's System Owner can reject a request in PendingSystemOwner state.");
            action = "RejectedBySystemOwner";
        }
        else
        {
            throw new BadRequestException($"Request cannot be rejected from status '{request.Status}'.");
        }
        request.Status = RequestStatus.Rejected;
        request.UpdatedAt = DateTime.UtcNow;
        _requestRepo.Update(request);
        await _auditRepo.AddAsync(new AuditEvent
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Timestamp = DateTime.UtcNow,
            ActorEmail = actorEmail,
            Action = action,
            OldStatus = oldStatus,
            NewStatus = RequestStatus.Rejected,
            Reason = dto.Reason   // Audit trail captures the rejection reason
        });
        await SaveTransitionWithAuditAsync();

        return MapToResponse(request);
    }
    private static bool IsHighRisk(AccessRequest request) =>
        request.Environment == AppEnvironment.Production
        || request.AccessLevel == AccessLevel.Admin;
    private async Task<User> GetUserOrThrowAsync(string email)
    {
        var user = await _context.Users.FindAsync(email)
            ?? throw new NotFoundException($"User '{email}' not found. Ensure the x-user-email header is a valid seeded user.");
        return user;
    }
    private async Task<bool> IsDirectManagerAsync(string managerEmail, string requesterEmail)
    {
        var requester = await GetUserOrThrowAsync(requesterEmail);
        return SameEmail(requester.ManagerEmail, managerEmail);
    }
    private async Task SaveTransitionWithAuditAsync()
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
                "A concurrent update was detected. The request has been modified by another process. " +
                "Please reload the latest version and retry.");
        }
    }
    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    private static bool SameEmail(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    private static AccessRequestResponse MapToResponse(AccessRequest r) => new()
    {
        Id = r.Id,
        ClientRequestId = r.ClientRequestId,
        RequesterEmail = r.RequesterEmail,
        ApplicationName = r.Application?.Name ?? string.Empty,
        Environment = r.Environment,
        AccessLevel = r.AccessLevel,
        Justification = r.Justification,
        Status = r.Status,
        PolicyVersion = r.PolicyVersion,
        Version = r.Version,
        IsHighRisk = r.Environment == AppEnvironment.Production || r.AccessLevel == AccessLevel.Admin,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
    private static AccessRequestDetailResponse MapToDetailResponse(AccessRequest r) => new()
    {
        Id = r.Id,
        ClientRequestId = r.ClientRequestId,
        RequesterEmail = r.RequesterEmail,
        ApplicationName = r.Application?.Name ?? string.Empty,
        Environment = r.Environment,
        AccessLevel = r.AccessLevel,
        Justification = r.Justification,
        Status = r.Status,
        PolicyVersion = r.PolicyVersion,
        Version = r.Version,
        IsHighRisk = r.Environment == AppEnvironment.Production || r.AccessLevel == AccessLevel.Admin,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        AuditTrail = r.AuditEvents
            .OrderBy(e => e.Timestamp)
            .Select(e => new AuditEventResponse
            {
                Id = e.Id,
                Timestamp = e.Timestamp,
                ActorEmail = e.ActorEmail,
                Action = e.Action,
                OldStatus = e.OldStatus,
                NewStatus = e.NewStatus,
                Reason = e.Reason
            })
    };
}
