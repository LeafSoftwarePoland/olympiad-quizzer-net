using Microsoft.AspNetCore.Mvc.Testing;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L2.Extensions;

[Trait(TestTiers.Tier, TestTiers.L2)]
public sealed class CorsExtensionsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string _configuredOrigin = "https://pages.example.com";

    [Fact]
    public async Task UseFrontendCors_ReturnsAccessControlAllowOriginHeader_WhenPreflightFromAllowedOrigin()
    {
        // Arrange
        const string preflightRoute = "/v1/questions";
        const string requestedMethod = "GET";

        HttpClient client = factory
            .WithWebHostBuilder(b => b.UseSetting("Cors:AllowedOrigin", _configuredOrigin))
            .CreateClient();
        HttpRequestMessage request = new(HttpMethod.Options, preflightRoute);
        request.Headers.Add("Origin", _configuredOrigin);
        request.Headers.Add("Access-Control-Request-Method", requestedMethod);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            $"Expected 'Access-Control-Allow-Origin' header to be present for allowed origin '{_configuredOrigin}'");
    }

    [Fact]
    public async Task UseFrontendCors_DoesNotReturnAccessControlAllowOriginHeader_WhenPreflightFromDisallowedOrigin()
    {
        // Arrange
        const string disallowedOrigin = "https://evil.example.com";
        const string preflightRoute = "/v1/questions";
        const string requestedMethod = "GET";

        HttpClient client = factory
            .WithWebHostBuilder(b => b.UseSetting("Cors:AllowedOrigin", _configuredOrigin))
            .CreateClient();
        HttpRequestMessage request = new(HttpMethod.Options, preflightRoute);
        request.Headers.Add("Origin", disallowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", requestedMethod);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.False(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            $"Expected no 'Access-Control-Allow-Origin' header for disallowed origin '{disallowedOrigin}'");
    }
}
