using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Repositories;
public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _context;

    public AuditRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(AuditEvent auditEvent)
    {
        await _context.AuditEvents.AddAsync(auditEvent);
    }
    public async Task<IEnumerable<AuditEvent>> GetByRequestIdAsync(Guid requestId)
    {
        return await _context.AuditEvents
            .Where(e => e.RequestId == requestId)
            .OrderBy(e => e.Timestamp)   // Chronological order for audit trail display
            .ToListAsync();
    }
}
