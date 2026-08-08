using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Tests;

public class ApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();

    [Fact]
    public async Task Healthz_Returns200AndOkTrue()
    {
        var resp = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"ok\":true", body.Replace(" ", ""));
    }

    [Fact]
    public async Task Questions_Returns200()
    {
        var resp = await _client.GetAsync("/api/questions");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Contains("application/json", resp.Content.Headers.ContentType?.MediaType ?? "");
    }

    [Fact]
    public async Task Questions_ReturnsSixQuestions()
    {
        var resp = await _client.GetAsync("/api/questions");
        var body = await resp.Content.ReadAsStringAsync();
        var questions = JsonSerializer.Deserialize<List<Question>>(body, JsonOptions.Default)!;
        Assert.Equal(6, questions.Count);
    }

    [Fact]
    public async Task Questions_PayloadIsCamelCase()
    {
        var resp = await _client.GetAsync("/api/questions");
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"correctAnswer\"", body);
        Assert.Contains("\"partialCredit\"", body);
        Assert.Contains("\"matchOptions\"",  body);
    }

    [Fact]
    public async Task Questions_DeserializesIntoSharedModel()
    {
        var resp = await _client.GetAsync("/api/questions");
        var body = await resp.Content.ReadAsStringAsync();
        var questions = JsonSerializer.Deserialize<List<Question>>(body, JsonOptions.Default)!;
        Assert.Equal(6, questions.Count);
        Assert.DoesNotContain(QuestionType.Unknown, questions.Select(q => q.Type));
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        var resp = await _client.GetAsync("/does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Cors_AllowsGitHubPagesOrigin()
    {
        var req = new HttpRequestMessage(HttpMethod.Options, "/api/questions");
        req.Headers.Add("Origin", "https://leafsoftwarepoland.github.io");
        req.Headers.Add("Access-Control-Request-Method", "GET");

        var resp = await _client.SendAsync(req);

        var allowOrigin = resp.Headers.TryGetValues("Access-Control-Allow-Origin", out var vals)
                          ? vals.FirstOrDefault()
                          : null;
        Assert.Equal("https://leafsoftwarepoland.github.io", allowOrigin);
    }

    [Fact]
    public async Task Cors_RejectsUnknownOrigin()
    {
        var req = new HttpRequestMessage(HttpMethod.Options, "/api/questions");
        req.Headers.Add("Origin", "https://evil.example");
        req.Headers.Add("Access-Control-Request-Method", "GET");

        var resp = await _client.SendAsync(req);

        Assert.False(resp.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
