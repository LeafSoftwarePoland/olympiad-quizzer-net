using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using OlympiadQuizzer.App.Api.Controllers;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
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
    public async Task Get_ReturnsOkWithQuestions_WhenNoFiltersGiven()
    {
        // Act
        IActionResult result = await _controller.Get([], [], [], [], null, CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<Question> questions = Assert.IsAssignableFrom<IReadOnlyList<Question>>(ok.Value);
        Assert.NotEmpty(questions);
    }

    [Fact]
    public async Task Get_ReturnsAtMostLimit_WhenLimitIsGiven()
    {
        // Arrange
        const int limit = 3;

        // Act
        IActionResult result = await _controller.Get([], [], [], [], limit, CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<Question> questions = Assert.IsAssignableFrom<IReadOnlyList<Question>>(ok.Value);
        Assert.True(questions.Count <= limit, $"Expected <= {limit} questions but got {questions.Count}");
    }

    [Fact]
    public async Task Get_ReturnsOkWithEmptyArray_WhenCategoryDoesNotExist()
    {
        // Arrange
        const string nonExistentCategory = "does_not_exist_category";

        // Act
        IActionResult result = await _controller.Get(
            [nonExistentCategory], [], [], [], null, CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        IReadOnlyList<Question> questions = Assert.IsAssignableFrom<IReadOnlyList<Question>>(ok.Value);
        Assert.Empty(questions);
    }

    [Fact]
    public async Task Get_BubblesException_WhenRepositoryThrowsUnanticipated()
    {
        // Arrange
        DivideByZeroException expected = new("unexpected repository failure");
        QuestionsController controller = new(
            new ThrowingRepository(expected),
            NullLogger<QuestionsController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        DivideByZeroException actual = await Assert.ThrowsAsync<DivideByZeroException>(
            () => controller.Get([], [], [], [], null, CancellationToken.None));

        // Assert
        Assert.Same(expected, actual);
    }

    private sealed class ThrowingRepository(Exception exception) : IQuestionRepository
    {
        public Task<IReadOnlyList<Question>> GetAsync(QuestionQuery query, CancellationToken cancellationToken)
            => throw exception;

        public Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken)
            => throw exception;
    }
}
