using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace OlympiadQuizzer.App.Api.Endpoints;

internal static class RobotsEndpoints
{
    internal static void Map(WebApplication app)
    {
        // robots.txt is host-scoped; a file served from the GitHub Pages origin cannot cover this one.
        app.MapGet("/robots.txt", () => Results.Text(
            "User-agent: *\nDisallow: /\n", "text/plain"));
    }
}
