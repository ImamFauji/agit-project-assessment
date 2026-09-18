using AccessRequestHub.Api.Models.Enums;

namespace AccessRequestHub.Api.Models.Entities;
public class User
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? ManagerEmail { get; set; }
    public ICollection<AccessRequest> RequestedAccessRequests { get; set; } = new List<AccessRequest>();
}
