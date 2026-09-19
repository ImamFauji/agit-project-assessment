using AccessRequestHub.Api.DTOs;
using AccessRequestHub.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestHub.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController(IApplicationRepository applications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> List() => Ok(
        (await applications.ListAsync()).Select(application => new ApplicationResponse
        {
            Id = application.Id,
            Name = application.Name
        }));
}
