using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Models.Enums;
using AccessRequestHub.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly DataContext _context;

    public RequestRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<AccessRequest?> GetByIdAsync(Guid id)
    {
        return await _context.AccessRequests
            .Include(r => r.Application)           
            .Include(r => r.AuditEvents)            
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    public async Task<AccessRequest?> GetByClientRequestIdAsync(string clientRequestId)
    {
        return await _context.AccessRequests
            .Include(r => r.Application)
            .FirstOrDefaultAsync(r => r.ClientRequestId == clientRequestId);
    }
    public async Task<IEnumerable<AccessRequest>> ListByRequesterAsync(string requesterEmail)
    {
        return await _context.AccessRequests
            .Include(r => r.Application)
            .Where(r => r.RequesterEmail == requesterEmail)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    public async Task<IEnumerable<AccessRequest>> ListPendingForManagerAsync(string managerEmail)
    {
        return await _context.AccessRequests
            .Include(r => r.Application)
            .Where(r => r.Status == RequestStatus.PendingManager
                        && _context.Users
                               .Where(u => u.ManagerEmail == managerEmail)
                               .Select(u => u.Email)
                               .Contains(r.RequesterEmail))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    public async Task<IEnumerable<AccessRequest>> ListPendingForSystemOwnerAsync(string ownerEmail)
    {
        return await _context.AccessRequests
            .Include(r => r.Application)
            .Where(r => r.Status == RequestStatus.PendingSystemOwner
                        && r.Application.OwnerEmail == ownerEmail)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    public async Task<IEnumerable<AccessRequest>> ListAllAsync()
    {
        return await _context.AccessRequests
            .AsNoTracking()
            .Include(r => r.Application)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    public async Task AddAsync(AccessRequest request)
    {
        await _context.AccessRequests.AddAsync(request);
    }
}
