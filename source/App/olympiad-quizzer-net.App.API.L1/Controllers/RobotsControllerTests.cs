using Microsoft.AspNetCore.Mvc;
using OlympiadQuizzer.App.Api.Controllers;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Controllers;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class RobotsControllerTests
{
    private readonly RobotsController _controller = new();

    [Fact]
    public void Get_ReturnsContentResultWithTextPlain()
    {
        IActionResult result = _controller.Get();

        ContentResult content = Assert.IsType<ContentResult>(result);
        Assert.Equal("text/plain", content.ContentType);
    }

    [Fact]
    public void Get_Body_DoesDisallowEverything()
    {
        IActionResult result = _controller.Get();

        ContentResult content = Assert.IsType<ContentResult>(result);
        Assert.Contains("Disallow: /", content.Content);
    }
}
