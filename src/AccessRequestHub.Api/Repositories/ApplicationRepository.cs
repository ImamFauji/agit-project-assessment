using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Repositories;

/// <summary>Database queries for the read-only application catalogue.</summary>
public sealed class ApplicationRepository(AppDbContext context) : IApplicationRepository
{
    public async Task<IReadOnlyList<Application>> ListAsync() =>
        await context.Applications.AsNoTracking().OrderBy(application => application.Name).ToListAsync();
}
