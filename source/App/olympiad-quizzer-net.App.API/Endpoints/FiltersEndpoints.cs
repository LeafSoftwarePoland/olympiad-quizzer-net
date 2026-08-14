using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;

namespace OlympiadQuizzer.App.Api.Endpoints;

internal static class FiltersEndpoints
{
    internal static void Map(WebApplication app)
    {
        app.MapGet("/api/filters", async (
            IQuestionRepository repository,
            CancellationToken cancellationToken) =>
        {
            FilterOptions options = await repository.GetFilterOptionsAsync(cancellationToken);
            return Results.Ok(options);
        });
    }
}
