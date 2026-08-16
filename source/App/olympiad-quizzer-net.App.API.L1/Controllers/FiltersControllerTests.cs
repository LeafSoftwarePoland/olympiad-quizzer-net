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
public sealed class FiltersControllerTests
{
    private readonly FiltersController _controller;

    public FiltersControllerTests()
    {
        _controller = new(
            ControllerHarness.RealBankRepository(),
            NullLogger<FiltersController>.Instance);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Get_ReturnsOkWithFilterOptions_WhenBankHasQuestions()
    {
        // Act
        IActionResult result = await _controller.Get(CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        FilterOptions options = Assert.IsType<FilterOptions>(ok.Value);
        Assert.NotEmpty(options.Categories);
        Assert.True(options.TotalQuestions > 0);
    }

    [Fact]
    public async Task Get_ReturnsCategoriesWithPositiveCounts_WhenBankHasQuestions()
    {
        // Act
        IActionResult result = await _controller.Get(CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        FilterOptions options = Assert.IsType<FilterOptions>(ok.Value);
        Assert.All(options.Categories, c => Assert.True(c.Count > 0));
    }

    [Fact]
    public async Task Get_BubblesException_WhenRepositoryThrowsUnanticipated()
    {
        // Arrange
        DivideByZeroException expected = new("unexpected repository failure");
        FiltersController controller = new(
            new ThrowingRepository(expected),
            NullLogger<FiltersController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        DivideByZeroException actual = await Assert.ThrowsAsync<DivideByZeroException>(
            () => controller.Get(CancellationToken.None));

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
