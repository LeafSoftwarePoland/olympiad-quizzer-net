using System.Net;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Extensions;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class CorsExtensionsTests : IClassFixture<ApiFactory>
{
    private const string _gitHubPagesOrigin = "https://leafsoftwarepoland.github.io";
    private const string _allowOriginHeader = "Access-Control-Allow-Origin";
    private const string _allowCredentials  = "Access-Control-Allow-Credentials";

    private readonly HttpClient _client;

    public CorsExtensionsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Preflight_FromGitHubPagesOrigin_DoesAllowOrigin()
    {
        HttpRequestMessage request = BuildPreflight(_gitHubPagesOrigin);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(_allowOriginHeader),
            "Expected Access-Control-Allow-Origin for GitHub Pages origin");
    }

    [Fact]
    public async Task Preflight_FromGitHubPagesOriginWithDifferentCasing_DoesAllowOrigin()
    {
        HttpRequestMessage request = BuildPreflight("https://LEAFSOFTWAREPOLAND.GITHUB.IO");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(_allowOriginHeader),
            "Origin matching must be case-insensitive");
    }

    [Fact]
    public async Task Preflight_FromLocalhostWithAnyPort_DoesAllowOrigin()
    {
        HttpRequestMessage request = BuildPreflight("http://localhost:5001");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(_allowOriginHeader),
            "localhost with any port must be allowed");
    }

    [Fact]
    public async Task Preflight_FromLoopbackIpAddress_DoesAllowOrigin()
    {
        HttpRequestMessage request = BuildPreflight("http://127.0.0.1:5000");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains(_allowOriginHeader),
            "127.0.0.1 loopback must be allowed");
    }

    [Fact]
    public async Task Preflight_FromUnknownOrigin_DoesNotEmitAllowOriginHeader()
    {
        HttpRequestMessage request = BuildPreflight("https://evil.example.com");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains(_allowOriginHeader),
            "Unknown origin must not receive Access-Control-Allow-Origin");
    }

    [Fact]
    public async Task Preflight_FromLookalikeOrigin_DoesNotEmitAllowOriginHeader()
    {
        HttpRequestMessage request = BuildPreflight("https://leafsoftwarepoland.github.io.evil.example");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains(_allowOriginHeader),
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
        HttpRequestMessage request = BuildPreflight(_gitHubPagesOrigin);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains(_allowCredentials),
            "Credentials header must be absent — no cookies, no auth");
    }

    [Fact]
    public async Task ActualGetRequest_FromAllowedOrigin_DoesIncludeAllowOriginHeader()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "/api/filters");
        request.Headers.Add("Origin", _gitHubPagesOrigin);

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains(_allowOriginHeader),
            "Actual GET from allowed origin must also carry the CORS header");
    }

    private static HttpRequestMessage BuildPreflight(string origin)
    {
        HttpRequestMessage request = new(HttpMethod.Options, "/api/questions");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return request;
    }
}
