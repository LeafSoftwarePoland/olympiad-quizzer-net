using Microsoft.AspNetCore.Mvc;
using OlympiadQuizzer.Core.Domain.Abstractions;
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
        [FromQuery(Name = "limit")]      int?     limit,
        CancellationToken cancellationToken)
    {
        if (limit.HasValue && (limit.Value < 1 || limit.Value > QuestionQuery.MaxLimit))
        {
            logger.LogWarning("Rejected out-of-range limit {Limit}", limit.Value);
            ModelState.AddModelError("limit", $"limit must be between 1 and {QuestionQuery.MaxLimit}.");
            return ValidationProblem();
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
        return Ok(questions);
    }
}
