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
        // Act
        IActionResult result = _controller.Get();

        // Assert
        ContentResult content = Assert.IsType<ContentResult>(result);
        Assert.Equal("text/plain", content.ContentType);
    }

    [Fact]
    public void Get_DoesDisallowAll_WhenBodyIsRead()
    {
        // Act
        IActionResult result = _controller.Get();

        // Assert
        ContentResult content = Assert.IsType<ContentResult>(result);
        Assert.Contains("Disallow: /", content.Content);
    }
}
