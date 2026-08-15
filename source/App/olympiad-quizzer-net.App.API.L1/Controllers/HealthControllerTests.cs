using Microsoft.AspNetCore.Mvc;
using OlympiadQuizzer.App.Api.Controllers;
using OlympiadQuizzer.Core.Tests.Common;
using System.Text.Json;

namespace OlympiadQuizzer.App.Api.L1.Controllers;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class HealthControllerTests
{
    private readonly HealthController _controller = new();

    [Fact]
    public void Get_ReturnsOkWithOkTrue()
    {
        // Act
        IActionResult result = _controller.Get();

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        string json = JsonSerializer.Serialize(ok.Value);
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public void Get_ReturnsCommitAsLocal_WhenRenderGitCommitVariableIsAbsent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("RENDER_GIT_COMMIT", null);

        // Act
        IActionResult result = _controller.Get();

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        string json = JsonSerializer.Serialize(ok.Value);
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.Equal("local", doc.RootElement.GetProperty("commit").GetString());
    }
}
