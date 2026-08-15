using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using OlympiadQuizzer.App.Api.Controllers;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Controllers;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class QuestionsControllerTests
{
    private readonly QuestionsController _controller;

    public QuestionsControllerTests()
    {
        _controller = new(
            ControllerHarness.RealBankRepository(),
            NullLogger<QuestionsController>.Instance);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Get_WithNoFilters_ReturnsOkWithNonEmptyArray()
    {
        IActionResult result = await _controller.Get([], [], [], [], null, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<Question> questions = Assert.IsAssignableFrom<IReadOnlyList<Question>>(ok.Value);
        Assert.NotEmpty(questions);
    }

    [Fact]
    public async Task Get_WithLimit_ReturnsAtMostLimit()
    {
        IActionResult result = await _controller.Get([], [], [], [], 3, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<Question> questions = Assert.IsAssignableFrom<IReadOnlyList<Question>>(ok.Value);
        Assert.True(questions.Count <= 3, $"Expected <= 3 questions but got {questions.Count}");
    }

    [Fact]
    public async Task Get_WithUnmatchedCategory_ReturnsOkWithEmptyArray()
    {
        IActionResult result = await _controller.Get(
            ["does_not_exist_category"], [], [], [], null, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<Question> questions = Assert.IsAssignableFrom<IReadOnlyList<Question>>(ok.Value);
        Assert.Empty(questions);
    }

    [Fact]
    public async Task Get_WithZeroLimit_ReturnsBadRequest()
    {
        IActionResult result = await _controller.Get([], [], [], [], 0, CancellationToken.None);

        ObjectResult objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        ValidationProblemDetails details = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.True(details.Errors.ContainsKey("limit"), "Expected a 'limit' validation error");
    }

    [Fact]
    public async Task Get_WithNegativeLimit_ReturnsBadRequest()
    {
        IActionResult result = await _controller.Get([], [], [], [], -1, CancellationToken.None);

        ObjectResult objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        ValidationProblemDetails details = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.True(details.Errors.ContainsKey("limit"), "Expected a 'limit' validation error");
    }

    [Fact]
    public async Task Get_WithLimitAboveMax_ReturnsBadRequest()
    {
        IActionResult result = await _controller.Get([], [], [], [], 31, CancellationToken.None);

        ObjectResult objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        ValidationProblemDetails details = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.True(details.Errors.ContainsKey("limit"), "Expected a 'limit' validation error");
    }
}
