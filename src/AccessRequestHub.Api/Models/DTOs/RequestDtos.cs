using AccessRequestHub.Api.Models.Enums;

namespace AccessRequestHub.Api.Models.DTOs;
public class CreateRequestDto
{
    public string ClientRequestId { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public AppEnvironment Environment { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public string Justification { get; set; } = string.Empty;
}
public class ApproveRequestDto
{
    public uint Version { get; set; }
}
public class RejectRequestDto
{
    public uint Version { get; set; }
    public string Reason { get; set; } = string.Empty;
}
