using System.Net;
using System.Text.Json;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Endpoints;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class HealthEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthz_ReturnsOkWithOkTrue()
    {
        HttpResponseMessage response = await _client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task GetHealthz_WithoutRenderGitCommitVariable_ReturnsCommitAsLocal()
    {
        HttpResponseMessage response = await _client.GetAsync("/healthz");

        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        string commit = doc.RootElement.GetProperty("commit").GetString();

        Assert.Equal("local", commit);
    }

    [Fact]
    public async Task GetHealthz_FromAllowedOrigin_DoesIncludeAllowOriginHeader()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "/healthz");
        request.Headers.Add("Origin", "https://leafsoftwarepoland.github.io");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Expected Access-Control-Allow-Origin header for allowed origin");
    }
}
