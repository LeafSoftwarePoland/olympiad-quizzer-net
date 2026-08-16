using Microsoft.AspNetCore.Mvc.Testing;
using OlympiadQuizzer.Core.Tests.Common;
using System.Net;
using System.Text.Json;

namespace OlympiadQuizzer.App.Api.L2.Controllers;

[Trait(TestTiers.Tier, TestTiers.L2)]
public sealed class QuestionsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_ReturnsOkWithArray_WhenCategoryParameterIsRepeated()
    {
        // Arrange
        const string firstCategory = "nonexistent_category_alpha";
        const string secondCategory = "nonexistent_category_beta";
        string route = $"/v1/questions?category={firstCategory}&category={secondCategory}";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsOkWithArray_WhenAlgorithmsParameterIsRepeated()
    {
        // Arrange
        const string firstAlgorithm = "nonexistent_algorithm_alpha";
        const string secondAlgorithm = "nonexistent_algorithm_beta";
        string route = $"/v1/questions?algorithms={firstAlgorithm}&algorithms={secondAlgorithm}";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsOkWithArray_WhenYearParameterIsRepeated()
    {
        // Arrange
        const int firstYear = 9997;
        const int secondYear = 9998;
        string route = $"/v1/questions?year={firstYear}&year={secondYear}";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsOkWithArray_WhenStageParameterIsRepeated()
    {
        // Arrange
        const string firstStage = "E1";
        const string secondStage = "E2";
        string route = $"/v1/questions?stage={firstStage}&stage={secondStage}";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public async Task Get_ReturnsBadRequestWithProblemDetailsMediaType_WhenLimitIsOutOfRange(int outOfRangeLimit)
    {
        // Arrange
        string route = $"/v1/questions?limit={outOfRangeLimit}";
        const string problemDetailsMediaType = "application/problem+json";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(problemDetailsMediaType, response.Content.Headers.ContentType.MediaType);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.False(doc.RootElement.TryGetProperty("code", out _));
    }

    [Fact]
    public async Task Get_ReturnsBadRequestWithProblemDetailsMediaType_WhenLimitIsNonNumeric()
    {
        // Arrange
        const string nonNumericLimit = "abc";
        string route = $"/v1/questions?limit={nonNumericLimit}";
        const string problemDetailsMediaType = "application/problem+json";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(problemDetailsMediaType, response.Content.Headers.ContentType.MediaType);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.False(doc.RootElement.TryGetProperty("code", out _));
    }

    [Fact]
    public async Task Get_ReturnsOkWithEmptyArray_WhenNoQuestionsMatchFilter()
    {
        // Arrange
        const string nonExistentCategory = "this_category_does_not_exist_in_bank_9999";
        string route = $"/v1/questions?category={nonExistentCategory}";
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }
}
