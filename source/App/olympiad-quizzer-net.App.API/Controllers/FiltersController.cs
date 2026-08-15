using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using OlympiadQuizzer.App.Api.Middleware;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Errors;
using OlympiadQuizzer.Core.Domain.Queries;

namespace OlympiadQuizzer.App.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public sealed class FiltersController(
    IQuestionRepository repository,
    ILogger<FiltersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            FilterOptions options = await repository.GetFilterOptionsAsync(cancellationToken);
            return Ok(options);
        }
        catch (SqliteException e)
        {
            logger.LogError(e, "Repository failure during GetFilterOptionsAsync.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
        catch (IOException e)
        {
            logger.LogError(e, "IO failure during GetFilterOptionsAsync.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException e)
        {
            logger.LogError(e, "Repository invalid operation during GetFilterOptionsAsync.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
        catch (JsonException e)
        {
            logger.LogError(e, "JSON failure during GetFilterOptionsAsync.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse(ErrorCodes.Unexpected, HttpContext.TraceIdentifier));
        }
    }
}
