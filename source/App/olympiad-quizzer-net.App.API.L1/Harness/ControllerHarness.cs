using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.App.Api.L1.Harness;

internal static class ControllerHarness
{
    internal const int DefaultSeed = 20260813;

    internal static SqliteQuestionRepository RealBankRepository()
    {
        string dbPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.db");
        QuestionBankOptions options = new() { DatabasePath = dbPath };
        return new SqliteQuestionRepository(
            Options.Create(options),
            new SeededShuffler(DefaultSeed),
            NullLogger<SqliteQuestionRepository>.Instance);
    }

    /// <summary>
    /// A no-infrastructure ProblemDetailsFactory that satisfies ControllerBase.ValidationProblem()
    /// without requiring the full MVC service pipeline.
    /// </summary>
    internal static ProblemDetailsFactory MinimalProblemDetailsFactory { get; } = new MinimalFactory();

    private sealed class MinimalFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            return new ProblemDetails
            {
                Status   = statusCode ?? StatusCodes.Status500InternalServerError,
                Title    = title,
                Type     = type,
                Detail   = detail,
                Instance = instance
            };
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            return new ValidationProblemDetails(modelStateDictionary)
            {
                Status   = statusCode ?? StatusCodes.Status400BadRequest,
                Title    = title ?? "One or more validation errors occurred.",
                Type     = type,
                Detail   = detail,
                Instance = instance
            };
        }
    }
}
