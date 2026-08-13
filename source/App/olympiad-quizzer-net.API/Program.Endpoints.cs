using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.Domain.Abstractions;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Api;

public partial class Program
{
    private static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/healthz", () => Results.Ok(new
        {
            ok     = true,
            commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local"
        }));

        // robots.txt is host-scoped; a file served from the GitHub Pages origin cannot cover this one.
        app.MapGet("/robots.txt", () => Results.Text(
            "User-agent: *\nDisallow: /\n", "text/plain"));

        app.MapGet("/api/filters", async (
            IQuestionRepository repository,
            CancellationToken cancellationToken) =>
        {
            FilterOptions options = await repository.GetFilterOptionsAsync(cancellationToken);
            return Results.Ok(options);
        });

        app.MapGet("/api/questions", async (
            [FromQuery(Name = "category")]   string[] category,
            [FromQuery(Name = "algorithms")] string[] algorithms,
            [FromQuery(Name = "year")]       int[]    year,
            [FromQuery(Name = "stage")]      string[] stage,
            [FromQuery(Name = "limit")]      int?     limit,
            IQuestionRepository repository,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            if (limit.HasValue && (limit.Value < 1 || limit.Value > QuestionQuery.MaxLimit))
            {
                logger.LogWarning("Rejected out-of-range limit {Limit}", limit.Value);
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["limit"] = new[] { $"limit must be between 1 and {QuestionQuery.MaxLimit}." }
                });
            }

            QuestionQuery query = new QuestionQuery
            {
                Categories = category.ToList(),
                Algorithms = algorithms.ToList(),
                Years      = year.ToList(),
                Stages     = stage.ToList(),
                Limit      = limit ?? QuestionQuery.DefaultLimit
            };

            IReadOnlyList<Question> questions = await repository.GetAsync(query, cancellationToken);
            return Results.Ok(questions);
        });
    }
}
