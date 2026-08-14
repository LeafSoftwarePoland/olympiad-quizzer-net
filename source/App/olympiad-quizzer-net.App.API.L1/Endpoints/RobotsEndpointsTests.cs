using System.Net;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Endpoints;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class RobotsEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public RobotsEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRobotsTxt_ReturnsPlainTextContentType()
    {
        HttpResponseMessage response = await _client.GetAsync("/robots.txt");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string mediaType = response.Content.Headers.ContentType.MediaType;
        Assert.Equal("text/plain", mediaType);
    }

    [Fact]
    public async Task GetRobotsTxt_Body_DoesDisallowEverything()
    {
        HttpResponseMessage response = await _client.GetAsync("/robots.txt");

        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Disallow: /", body);
    }
}
