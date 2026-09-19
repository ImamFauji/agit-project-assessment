using AccessRequestHub.Api.Models.Entities;

namespace AccessRequestHub.Api.Repositories.Interfaces;
public interface IAuditRepository
{
    Task AddAsync(AuditEvent auditEvent);
}
