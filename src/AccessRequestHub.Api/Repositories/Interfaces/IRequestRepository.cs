using AccessRequestHub.Api.Models.Entities;

namespace AccessRequestHub.Api.Repositories.Interfaces;
public interface IRequestRepository
{
    Task<AccessRequest?> GetByIdAsync(Guid id);
    Task<AccessRequest?> GetByClientRequestIdAsync(string clientRequestId);
    Task<IEnumerable<AccessRequest>> ListByRequesterAsync(string requesterEmail);
    Task<IEnumerable<AccessRequest>> ListPendingForManagerAsync(string managerEmail);
    Task<IEnumerable<AccessRequest>> ListPendingForSystemOwnerAsync(string ownerEmail);
    Task<IEnumerable<AccessRequest>> ListAllAsync();
    Task AddAsync(AccessRequest request);
}
