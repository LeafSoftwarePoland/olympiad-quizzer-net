using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Errors;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using System.Net;
using System.Text.Json;

namespace OlympiadQuizzer.App.Api.L2.Middleware;

[Trait(TestTiers.Tier, TestTiers.L2)]
public sealed class GlobalExceptionMiddlewareTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task InvokeAsync_Returns500WithUnexpectedCode_WhenRepositoryThrowsUnanticipated()
    {
        // Arrange
        Mock<IQuestionRepository> throwingRepository = new();
        throwingRepository
            .Setup(r => r.GetAsync(It.IsAny<QuestionQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DivideByZeroException("unanticipated fault"));
        throwingRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DivideByZeroException("unanticipated fault"));

        await using WebApplicationFactory<Program> faultFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor descriptor = services.Single(
                    d => d.ServiceType == typeof(IQuestionRepository));
                services.Remove(descriptor);
                services.AddSingleton<IQuestionRepository>(_ => throwingRepository.Object);
            });
        });

        HttpClient client = faultFactory.CreateClient();
        const string questionsRoute = "/v1/questions";
        const string expectedCode = ErrorCodes.Unexpected;
        const string expectedContentType = "application/json";

        // Act
        HttpResponseMessage response = await client.GetAsync(questionsRoute);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(expectedContentType, response.Content.Headers.ContentType.MediaType);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        Assert.Equal(expectedCode, doc.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(doc.RootElement.GetProperty("requestId").GetString()));
    }
}
