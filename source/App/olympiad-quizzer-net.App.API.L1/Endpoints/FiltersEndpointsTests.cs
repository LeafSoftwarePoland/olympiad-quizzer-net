using System.Net;
using System.Text.Json;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.App.Api.L1.Endpoints;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class FiltersEndpointsTests(FilteringApiFactory factory) : IClassFixture<FilteringApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetFilters_ReturnsOkWithCamelCasePayload()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/filters");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("categories", out _), "Expected 'categories'");
        Assert.True(doc.RootElement.TryGetProperty("totalQuestions", out _), "Expected 'totalQuestions'");
    }

    [Fact]
    public async Task GetFilters_ReturnsCategoriesWithCounts()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/filters");

        string body = await response.Content.ReadAsStringAsync();
        FilterOptions options = JsonSerializer.Deserialize<FilterOptions>(body, JsonOptions.Default);

        Assert.NotNull(options);
        Assert.NotEmpty(options.Categories);
        Assert.All(options.Categories, c => Assert.True(c.Count > 0));
    }

    [Fact]
    public async Task GetFilters_ReturnsOnlyValuesPresentInBank()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/filters");

        string body = await response.Content.ReadAsStringAsync();
        FilterOptions options = JsonSerializer.Deserialize<FilterOptions>(body, JsonOptions.Default);

        IEnumerable<string> categoryValues = options.Categories.Select(c => c.Value);
        Assert.DoesNotContain("grafy_drzewa", categoryValues);
    }

    [Fact]
    public async Task GetFilters_ReturnsTotalQuestionCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/filters");

        string body = await response.Content.ReadAsStringAsync();
        FilterOptions options = JsonSerializer.Deserialize<FilterOptions>(body, JsonOptions.Default);

        Assert.Equal(15, options.TotalQuestions);
    }

    [Fact]
    public async Task GetFilters_CategoryFilterUsedAsQueryParameter_ReturnsMatchingQuestions()
    {
        HttpResponseMessage filtersResponse = await _client.GetAsync("/api/filters");
        string filtersBody = await filtersResponse.Content.ReadAsStringAsync();
        FilterOptions options = JsonSerializer.Deserialize<FilterOptions>(filtersBody, JsonOptions.Default);

        string firstCategory = options.Categories[0].Value;
        HttpResponseMessage questionsResponse = await _client.GetAsync(
            $"/api/questions?category={firstCategory}");

        Assert.Equal(HttpStatusCode.OK, questionsResponse.StatusCode);
        string questionsBody = await questionsResponse.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(questionsBody);
        Assert.True(doc.RootElement.GetArrayLength() > 0,
            $"Category '{firstCategory}' from /api/filters returned no questions from /api/questions");
    }
}
