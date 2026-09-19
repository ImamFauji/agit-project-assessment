using AccessRequestHub.Api.Models.Enums;

namespace AccessRequestHub.Api.DTOs;
public class AccessRequestResponse
{
    public Guid Id { get; set; }
    public string ClientRequestId { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;

    public AppEnvironment Environment { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public string Justification { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public string PolicyVersion { get; set; } = string.Empty;
    public uint Version { get; set; }

    public bool IsHighRisk { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class AuditEventResponse
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActorEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public RequestStatus? OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}
public class AccessRequestDetailResponse : AccessRequestResponse
{
    public IEnumerable<AuditEventResponse> AuditTrail { get; set; } = new List<AuditEventResponse>();
}
