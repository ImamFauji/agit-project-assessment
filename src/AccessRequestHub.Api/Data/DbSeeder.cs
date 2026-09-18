using AccessRequestHub.Api.Models.Entities;
using AccessRequestHub.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        if (await context.Applications.AnyAsync())
            return;
        var crmId = Guid.NewGuid();
        var financeId = Guid.NewGuid();

        var applications = new List<Models.Entities.Application>
        {
            new()
            {
                Id = crmId,
                Name = "CRM",
                OwnerEmail = "carol@example.local"
            },
            new()
            {
                Id = financeId,
                Name = "Finance Portal",
                OwnerEmail = "dana@example.local"
            }
        };

        await context.Applications.AddRangeAsync(applications);
        var users = new List<User>
        {
            new()
            {
                Email = "alice@example.local",
                Name = "Alice",
                Role = UserRole.Requester,
                ManagerEmail = "bob@example.local"   // Alice reports to Bob
            },
            new()
            {
                Email = "bob@example.local",
                Name = "Bob",
                Role = UserRole.Manager,
                ManagerEmail = null                  // Bob has no manager in this system
            },
            new()
            {
                Email = "carol@example.local",
                Name = "Carol",
                Role = UserRole.SystemOwner,
                ManagerEmail = null                  // Carol is a system owner (CRM)
            },
            new()
            {
                Email = "dana@example.local",
                Name = "Dana",
                Role = UserRole.SystemOwner,
                ManagerEmail = null                  // Dana is a system owner (Finance Portal)
            },
            new()
            {
                Email = "erin@example.local",
                Name = "Erin",
                Role = UserRole.Auditor,
                ManagerEmail = null                  // Erin is the auditor
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
