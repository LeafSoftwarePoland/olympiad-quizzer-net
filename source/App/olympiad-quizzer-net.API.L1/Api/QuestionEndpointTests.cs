using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Domain.Serialization;

namespace OlympiadQuizzer.Api.L1.Api;

[Trait("Tier", "L1")]
public sealed class QuestionEndpointTests : IClassFixture<FilteringApiFactory>
{
    private const string CategoryRecursion = "rekurencja";
    private const string CategorySorting   = "sortowanie";

    private readonly HttpClient _client;

    public QuestionEndpointTests(FilteringApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetQuestions_WithNoQueryString_ReturnsOkAndJsonArray()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetQuestions_ResponseIsCamelCase()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=1");
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement first = doc.RootElement[0];

        Assert.True(first.TryGetProperty("correctAnswer", out _), "Expected camelCase 'correctAnswer'");
        Assert.True(first.TryGetProperty("sourceUrl", out _), "Expected camelCase 'sourceUrl'");
    }

    [Fact]
    public async Task GetQuestions_ResponseDeserializesIntoDomainQuestion()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=1");

        string body = await response.Content.ReadAsStringAsync();
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(body, JsonOptions.Default);

        Assert.NotNull(questions);
        Assert.NotEmpty(questions);
        Assert.True(questions[0].Id > 0);
    }

    [Fact]
    public async Task GetQuestions_ResponseIncludesCorrectAnswers()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=1");

        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement first = doc.RootElement[0];

        Assert.True(first.TryGetProperty("correctAnswer", out JsonElement answer),
            "Expected correctAnswer in response");
        Assert.NotEqual(JsonValueKind.Null, answer.ValueKind);
    }

    [Fact]
    public async Task GetQuestions_WithSingleCategory_ReturnsOnlyThatCategory()
    {
        HttpResponseMessage response = await _client.GetAsync(
            $"/api/questions?category={CategoryRecursion}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(body, JsonOptions.Default);

        Assert.All(questions, q => Assert.Contains(CategoryRecursion, q.Category));
    }

    [Fact]
    public async Task GetQuestions_WithRepeatedCategoryParameter_AppliesOrWithinType()
    {
        HttpResponseMessage response = await _client.GetAsync(
            $"/api/questions?category={CategoryRecursion}&category={CategorySorting}&limit=30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string body = await response.Content.ReadAsStringAsync();
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(body, JsonOptions.Default);

        Assert.All(questions, q =>
            Assert.True(
                q.Category.Contains(CategoryRecursion) || q.Category.Contains(CategorySorting),
                $"Question {q.Id} has neither category"));
    }

    [Fact]
    public async Task GetQuestions_WithLimit_ReturnsAtMostLimit()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetArrayLength() <= 3);
    }

    [Fact]
    public async Task GetQuestions_WithoutLimit_ReturnsAtMostThirty()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions");

        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetArrayLength() <= 30);
    }

    [Fact]
    public async Task GetQuestions_WithFiltersMatchingNothing_ReturnsOkWithEmptyArray()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?category=does_not_exist");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetQuestions_WithLimitZero_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_WithNegativeLimit_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_WithLimitAboveThirty_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=31");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_WithNonNumericLimit_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_WithNonNumericYear_ReturnsBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?year=notanumber");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_BadRequestBodyIsProblemJson()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?limit=0");

        string mediaType = response.Content.Headers.ContentType.MediaType;
        Assert.Equal("application/problem+json", mediaType);
    }

    [Fact]
    public async Task GetQuestions_WithUnknownQueryParameter_IgnoresItAndReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions?foo=bar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_ExposesNoSetCookieHeader()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/questions");

        Assert.False(response.Headers.Contains("Set-Cookie"),
            "Stateless API must not set cookies");
    }

    [Fact]
    public async Task GetUnknownRoute_ReturnsNotFound()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostQuestions_ReturnsMethodNotAllowed()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/questions",
            new StringContent(string.Empty));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_WithPreCancelledToken_ThrowsOperationCancelled()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _client.GetAsync("/api/questions", cts.Token));
    }
}
