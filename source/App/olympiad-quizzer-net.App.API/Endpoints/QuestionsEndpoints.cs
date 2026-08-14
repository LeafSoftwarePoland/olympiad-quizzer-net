using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.App.Api.Endpoints;

internal sealed class QuestionsEndpoints
{
    internal static void Map(WebApplication app)
    {
        app.MapGet("/api/questions", async (
            [FromQuery(Name = "category")]   string[] category,
            [FromQuery(Name = "algorithms")] string[] algorithms,
            [FromQuery(Name = "year")]       int[]    year,
            [FromQuery(Name = "stage")]      string[] stage,
            [FromQuery(Name = "limit")]      int?     limit,
            IQuestionRepository repository,
            ILogger<QuestionsEndpoints> logger,
            CancellationToken cancellationToken) =>
        {
            if (limit.HasValue && (limit.Value < 1 || limit.Value > QuestionQuery.MaxLimit))
            {
                logger.LogWarning("Rejected out-of-range limit {Limit}", limit.Value);
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["limit"] = [$"limit must be between 1 and {QuestionQuery.MaxLimit}."]
                });
            }

            QuestionQuery query = new()
            {
                Categories = [.. category],
                Algorithms = [.. algorithms],
                Years      = [.. year],
                Stages     = [.. stage],
                Limit      = limit ?? QuestionQuery.DefaultLimit
            };

            IReadOnlyList<Question> questions = await repository.GetAsync(query, cancellationToken);
            return Results.Ok(questions);
        });
    }
}
