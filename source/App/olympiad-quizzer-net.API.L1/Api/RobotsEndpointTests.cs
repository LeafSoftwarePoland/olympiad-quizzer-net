using System.Net;
using System.Net.Http;
using OlympiadQuizzer.Api.L1.Harness;

namespace OlympiadQuizzer.Api.L1.Api;

[Trait("Tier", "L1")]
public sealed class RobotsEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public RobotsEndpointTests(ApiFactory factory)
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
    public async Task GetRobotsTxt_DisallowsEverything()
    {
        HttpResponseMessage response = await _client.GetAsync("/robots.txt");

        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Disallow: /", body);
    }
}
