using Microsoft.AspNetCore.Mvc;
using OlympiadQuizzer.App.Api.Controllers;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Controllers;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class FiltersControllerTests
{
    private readonly FiltersController _controller;

    public FiltersControllerTests()
    {
        _controller = new(ControllerHarness.RealBankRepository());
    }

    [Fact]
    public async Task Get_ReturnsOkWithFilterOptions()
    {
        IActionResult result = await _controller.Get(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        FilterOptions options = Assert.IsType<FilterOptions>(ok.Value);
        Assert.NotEmpty(options.Categories);
        Assert.True(options.TotalQuestions > 0);
    }

    [Fact]
    public async Task Get_ReturnsCategoriesWithPositiveCounts()
    {
        IActionResult result = await _controller.Get(CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        FilterOptions options = Assert.IsType<FilterOptions>(ok.Value);
        Assert.All(options.Categories, c => Assert.True(c.Count > 0));
    }
}
