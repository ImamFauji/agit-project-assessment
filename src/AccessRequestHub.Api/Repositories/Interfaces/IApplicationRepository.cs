using AccessRequestHub.Api.Models.Entities;

namespace AccessRequestHub.Api.Repositories.Interfaces;

public interface IApplicationRepository
{
    Task<IReadOnlyList<Application>> ListAsync();
}
