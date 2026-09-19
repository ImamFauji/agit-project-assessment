using Microsoft.AspNetCore.Mvc;

namespace AccessRequestHub.Api.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();
}
