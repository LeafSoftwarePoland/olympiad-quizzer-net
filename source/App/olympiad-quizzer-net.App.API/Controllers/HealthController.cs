using Microsoft.AspNetCore.Mvc;

namespace OlympiadQuizzer.App.Api.Controllers;

[ApiController]
[Route("healthz")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            ok     = true,
            commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local"
        });
    }
}
