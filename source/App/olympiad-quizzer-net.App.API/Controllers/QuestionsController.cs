using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using OlympiadQuizzer.App.Api.Middleware;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Errors;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.App.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public sealed class QuestionsController(
    IQuestionRepository repository,
    ILogger<QuestionsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery(Name = "category")]   string[] category,
        [FromQuery(Name = "algorithms")] string[] algorithms,
        [FromQuery(Name = "year")]       int[]    year,
        [FromQuery(Name = "stage")]      string[] stage,
        [FromQuery(Name = "limit")] [Range(1, QuestionQuery.MaxLimit)] int? limit,
        CancellationToken cancellationToken)
    {
        QuestionQuery query = new()
        {
            Categories = [.. category],
            Algorithms = [.. algorithms],
            Years      = [.. year],
            Stages     = [.. stage],
            Limit      = limit ?? QuestionQuery.DefaultLimit
        };

        try
        {
            IReadOnlyList<Question> questions = await repository.GetAsync(query, cancellationToken);
            return Ok(questions);
        }
        catch (SqliteException e)
        {
            logger.LogError(e,
                "Repository failure during GetAsync. limit={Limit} categories={Categories} " +
                "algorithms={Algorithms} years={Years} stages={Stages}",
                query.Limit, string.Join(",", query.Categories), string.Join(",", query.Algorithms),
                string.Join(",", query.Years), string.Join(",", query.Stages));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
        catch (IOException e)
        {
            logger.LogError(e,
                "IO failure during GetAsync. limit={Limit} categories={Categories} " +
                "algorithms={Algorithms} years={Years} stages={Stages}",
                query.Limit, string.Join(",", query.Categories), string.Join(",", query.Algorithms),
                string.Join(",", query.Years), string.Join(",", query.Stages));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException e)
        {
            logger.LogError(e,
                "Repository invalid operation during GetAsync. limit={Limit} categories={Categories} " +
                "algorithms={Algorithms} years={Years} stages={Stages}",
                query.Limit, string.Join(",", query.Categories), string.Join(",", query.Algorithms),
                string.Join(",", query.Years), string.Join(",", query.Stages));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
        catch (JsonException e)
        {
            logger.LogError(e,
                "JSON failure during GetAsync. limit={Limit} categories={Categories} " +
                "algorithms={Algorithms} years={Years} stages={Stages}",
                query.Limit, string.Join(",", query.Categories), string.Join(",", query.Algorithms),
                string.Join(",", query.Years), string.Join(",", query.Stages));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
    }
}
