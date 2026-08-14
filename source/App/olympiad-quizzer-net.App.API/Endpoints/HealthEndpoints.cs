using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace OlympiadQuizzer.App.Api.Endpoints;

internal static class HealthEndpoints
{
    internal static void Map(WebApplication app)
    {
        app.MapGet("/healthz", () => Results.Ok(new
        {
            ok     = true,
            commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local"
        }));
    }
}
