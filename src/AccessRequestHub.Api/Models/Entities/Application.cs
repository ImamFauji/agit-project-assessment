namespace AccessRequestHub.Api.Models.Entities;
public class Application
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
}
