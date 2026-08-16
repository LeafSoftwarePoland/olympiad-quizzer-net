using Microsoft.AspNetCore.Mvc.Testing;
using OlympiadQuizzer.Core.Tests.Common;
using System.Net;

namespace OlympiadQuizzer.App.Api.L2.Controllers;

[Trait(TestTiers.Tier, TestTiers.L2)]
public sealed class RobotsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_ReturnsOkWithTextPlain_WhenRobotsTxtIsRequestedAtHostRoot()
    {
        // Arrange
        const string robotsRoute = "/robots.txt";
        const string textPlainMediaType = "text/plain";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(robotsRoute);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(textPlainMediaType, response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenRobotsTxtIsRequestedUnderVersionSegment()
    {
        // Arrange
        const string versionedRobotsRoute = "/v1/robots.txt";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(versionedRobotsRoute);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
