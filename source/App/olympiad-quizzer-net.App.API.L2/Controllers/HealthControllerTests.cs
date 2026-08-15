using Microsoft.AspNetCore.Mvc.Testing;
using OlympiadQuizzer.Core.Tests.Common;
using System.Net;

namespace OlympiadQuizzer.App.Api.L2.Controllers;

[Trait(TestTiers.Tier, TestTiers.L2)]
public sealed class HealthControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_ReturnsOk_WhenHealthzIsRequestedAtHostRoot()
    {
        // Arrange
        const string healthzRoute = "/healthz";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(healthzRoute);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenHealthzIsRequestedUnderVersionSegment()
    {
        // Arrange
        const string versionedHealthzRoute = "/v1/healthz";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(versionedHealthzRoute);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
