using AccessRequestHub.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Data;
public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.OwnerEmail).IsRequired().HasMaxLength(256);
            entity.HasIndex(a => a.Name).IsUnique();
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Email);
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
            entity.Property(u => u.ManagerEmail).HasMaxLength(256);
            entity.HasMany(u => u.RequestedAccessRequests)
                  .WithOne()
                  .HasForeignKey(r => r.RequesterEmail)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AccessRequest>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.ClientRequestId).IsUnique();
            entity.Property(r => r.ClientRequestId).IsRequired().HasMaxLength(256);

            entity.Property(r => r.RequesterEmail).IsRequired().HasMaxLength(256);
            entity.Property(r => r.Justification).IsRequired().HasMaxLength(2000);
            entity.Property(r => r.PolicyVersion).IsRequired().HasMaxLength(20).HasDefaultValue("v1");
            entity.Property(r => r.Version)
                  .HasColumnName("xmin")
                  .HasColumnType("xid")
                  .ValueGeneratedOnAddOrUpdate()
                  .IsConcurrencyToken();
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_AccessRequests_Justification_NotBlank",
                "length(btrim(\"Justification\")) > 0"));
            entity.HasOne(r => r.Application)
                  .WithMany(a => a.AccessRequests)
                  .HasForeignKey(r => r.ApplicationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(r => r.AuditEvents)
                  .WithOne(e => e.AccessRequest)
                  .HasForeignKey(e => e.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActorEmail).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(2000);
            entity.HasIndex(e => e.RequestId);
        });
    }
}
