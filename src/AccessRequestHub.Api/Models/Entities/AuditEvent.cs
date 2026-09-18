using AccessRequestHub.Api.Models.Enums;

namespace AccessRequestHub.Api.Models.Entities;
public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ActorEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public RequestStatus? OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public AccessRequest AccessRequest { get; set; } = null!;
}
