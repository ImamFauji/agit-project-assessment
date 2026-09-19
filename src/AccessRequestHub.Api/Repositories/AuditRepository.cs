using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Repositories.Interfaces;

namespace AccessRequestHub.Api.Repositories;
public class AuditRepository : IAuditRepository
{
    private readonly DataContext _context;

    public AuditRepository(DataContext context)
    {
        _context = context;
    }
    public async Task AddAsync(AuditEvent auditEvent)
    {
        await _context.AuditEvents.AddAsync(auditEvent);
    }
}
