using Microsoft.AspNetCore.Mvc;

namespace OlympiadQuizzer.App.Api.Controllers;

[ApiController]
[Route("robots.txt")]
public sealed class RobotsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Content("User-agent: *\nDisallow: /\n", "text/plain");
    }
}
