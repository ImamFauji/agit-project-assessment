using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Exceptions;
using AccessRequestHub.Api.DTOs;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Models.Enums;
using AccessRequestHub.Api.Repositories;
using AccessRequestHub.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace AccessRequestHub.Api.Tests;

public sealed class RequestServiceTests
{
    private static async Task<(RequestService Service, Guid CrmId)> CreateSutAsync()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var db = new DataContext(options);
        var crmId = Guid.NewGuid();
        db.Users.AddRange(
            new User { Email = "alice@example.local", Name = "Alice", Role = UserRole.Requester, ManagerEmail = "bob@example.local" },
            new User { Email = "bob@example.local", Name = "Bob", Role = UserRole.Manager },
            new User { Email = "carol@example.local", Name = "Carol", Role = UserRole.SystemOwner },
            new User { Email = "erin@example.local", Name = "Erin", Role = UserRole.Auditor });
        db.Applications.Add(new Application { Id = crmId, Name = "CRM", OwnerEmail = "carol@example.local" });
        await db.SaveChangesAsync();
        return (new RequestService(new RequestRepository(db), new AuditRepository(db), db), crmId);
    }

    [Fact]
    public async Task Standard_request_is_approved_by_direct_manager()
    {
        var (service, appId) = await CreateSutAsync();
        var created = await service.CreateRequestAsync("alice@example.local", new CreateRequestDto { ClientRequestId = "standard", ApplicationId = appId, Environment = AppEnvironment.NonProduction, AccessLevel = AccessLevel.Read, Justification = "Support work" });
        var approved = await service.ApproveRequestAsync("bob@example.local", created.Response.Id, new ApproveRequestDto { Version = created.Response.Version });
        Assert.Equal(RequestStatus.Approved, approved.Status);
    }

    [Fact]
    public async Task High_risk_request_requires_system_owner()
    {
        var (service, appId) = await CreateSutAsync();
        var created = await service.CreateRequestAsync("alice@example.local", new CreateRequestDto { ClientRequestId = "high", ApplicationId = appId, Environment = AppEnvironment.Production, AccessLevel = AccessLevel.Read, Justification = "Production support" });
        var pending = await service.ApproveRequestAsync("bob@example.local", created.Response.Id, new ApproveRequestDto { Version = created.Response.Version });
        Assert.Equal(RequestStatus.PendingSystemOwner, pending.Status);
    }

    [Fact]
    public async Task Create_is_idempotent_and_unauthorized_approval_is_forbidden()
    {
        var (service, appId) = await CreateSutAsync();
        var dto = new CreateRequestDto { ClientRequestId = "same-id", ApplicationId = appId, Environment = AppEnvironment.NonProduction, AccessLevel = AccessLevel.Read, Justification = "Needed" };
        var first = await service.CreateRequestAsync("alice@example.local", dto);
        var retry = await service.CreateRequestAsync("alice@example.local", dto);
        Assert.False(retry.WasCreated); Assert.Equal(first.Response.Id, retry.Response.Id);
        await Assert.ThrowsAsync<ForbiddenException>(() => service.ApproveRequestAsync("carol@example.local", first.Response.Id, new ApproveRequestDto { Version = first.Response.Version }));
    }

    [Fact]
    public async Task Stale_version_is_a_conflict()
    {
        var (service, appId) = await CreateSutAsync();
        var created = await service.CreateRequestAsync("alice@example.local", new CreateRequestDto { ClientRequestId = "stale", ApplicationId = appId, Environment = AppEnvironment.NonProduction, AccessLevel = AccessLevel.Read, Justification = "Needed" });
        await Assert.ThrowsAsync<ConcurrencyException>(() => service.ApproveRequestAsync("bob@example.local", created.Response.Id, new ApproveRequestDto { Version = created.Response.Version + 1 }));
    }
}
