using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Repositories;

public sealed class ApplicationRepository(DataContext context) : IApplicationRepository
{
    public async Task<IReadOnlyList<Application>> ListAsync() =>
        await context.Applications
                .AsNoTracking()
                .OrderBy(application => application.Name)
                .ToListAsync();
}
