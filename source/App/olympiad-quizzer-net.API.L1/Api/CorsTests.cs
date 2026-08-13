using System.Net;
using System.Net.Http;
using OlympiadQuizzer.Api.L1.Harness;

namespace OlympiadQuizzer.Api.L1.Api;

[Trait("Tier", "L1")]
public sealed class CorsTests : IClassFixture<ApiFactory>
{
    private const string GitHubPagesOrigin = "https://leafsoftwarepoland.github.io";
    private const string AllowOriginHeader = "Access-Control-Allow-Origin";
    private const string AllowCredentials  = "Access-Control-Allow-Credentials";

    private readonly HttpClient _client;

    public CorsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_FromGitHubPagesOrigin_AllowsOrigin()
    {
        HttpRequestMessage request = BuildPreflight(GitHubPagesOrigin);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(AllowOriginHeader),
            "Expected Access-Control-Allow-Origin for GitHub Pages origin");
    }

    [Fact]
    public async Task Preflight_FromGitHubPagesOriginWithDifferentCasing_AllowsOrigin()
    {
        HttpRequestMessage request = BuildPreflight("https://LEAFSOFTWAREPOLAND.GITHUB.IO");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(AllowOriginHeader),
            "Origin matching must be case-insensitive");
    }

    [Fact]
    public async Task Preflight_FromLocalhostWithAnyPort_AllowsOrigin()
    {
        HttpRequestMessage request = BuildPreflight("http://localhost:5001");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(AllowOriginHeader),
            "localhost with any port must be allowed");
    }

    [Fact]
    public async Task Preflight_FromLoopbackIpAddress_AllowsOrigin()
    {
        HttpRequestMessage request = BuildPreflight("http://127.0.0.1:5000");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(AllowOriginHeader),
            "127.0.0.1 loopback must be allowed");
    }

    [Fact]
    public async Task Preflight_FromUnknownOrigin_DoesNotEmitAllowOriginHeader()
    {
        HttpRequestMessage request = BuildPreflight("https://evil.example.com");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains(AllowOriginHeader),
            "Unknown origin must not receive Access-Control-Allow-Origin");
    }

    [Fact]
    public async Task Preflight_FromLookalikeOrigin_DoesNotEmitAllowOriginHeader()
    {
        HttpRequestMessage request = BuildPreflight("https://leafsoftwarepoland.github.io.evil.example");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains(AllowOriginHeader),
            "Lookalike origin must not pass substring matching");
    }

    [Fact]
    public async Task Preflight_FromMalformedOrigin_DoesNotThrow()
    {
        HttpRequestMessage request = BuildPreflight("not-a-valid-origin");

        Exception exception = await Record.ExceptionAsync(() => _client.SendAsync(request));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Response_DoesNotAllowCredentials()
    {
        HttpRequestMessage request = BuildPreflight(GitHubPagesOrigin);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains(AllowCredentials),
            "Credentials header must be absent — no cookies, no auth");
    }

    [Fact]
    public async Task ActualGetRequest_FromAllowedOrigin_IncludesAllowOriginHeader()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/filters");
        request.Headers.Add("Origin", GitHubPagesOrigin);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains(AllowOriginHeader),
            "Actual GET from allowed origin must also carry the CORS header");
    }

    private static HttpRequestMessage BuildPreflight(string origin)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Options, "/api/questions");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return request;
    }
}
