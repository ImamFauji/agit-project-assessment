using AccessRequestHub.Api.Models.Enums;

namespace AccessRequestHub.Api.Models.Entities;
public class AccessRequest
{
    public Guid Id { get; set; }
    public string ClientRequestId { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public AppEnvironment Environment { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public string Justification { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.PendingManager;
    public string PolicyVersion { get; set; } = "v1";
    public uint Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Application Application { get; set; } = null!;
    public ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();
}
