using Microsoft.AspNetCore.Mvc;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;

namespace OlympiadQuizzer.App.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public sealed class FiltersController(IQuestionRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        FilterOptions options = await repository.GetFilterOptionsAsync(cancellationToken);
        return Ok(options);
    }
}
