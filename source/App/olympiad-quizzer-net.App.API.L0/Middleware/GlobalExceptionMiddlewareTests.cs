using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.App.Api.Middleware;
using OlympiadQuizzer.Core.Domain.Errors;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using System.Text.Json;

namespace OlympiadQuizzer.App.Api.L0.Middleware;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CallsNext_WhenNoExceptionThrown()
    {
        // Arrange
        bool nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotLog_WhenNoExceptionThrown()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task InvokeAsync_Returns500_WhenNextThrowsException()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("boom");
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsJsonContentType_WhenNextThrowsException()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("boom");
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsUnexpectedCode_WhenNextThrowsException()
    {
        // Arrange
        const string expectedTraceId = "test-trace-id-01";
        RequestDelegate next = _ => throw new InvalidOperationException("boom");
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();
        context.TraceIdentifier = expectedTraceId;
        MemoryStream responseBody = new();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseBody.Seek(0, SeekOrigin.Begin);
        string json = new StreamReader(responseBody).ReadToEnd();
        using JsonDocument doc = JsonDocument.Parse(json);
        Assert.Equal(ErrorCodes.Unexpected, doc.RootElement.GetProperty("code").GetString());
        Assert.Equal(expectedTraceId, doc.RootElement.GetProperty("requestId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_LogsError_WhenNextThrowsException()
    {
        // Arrange
        Exception thrownException = new InvalidOperationException("something broke");
        RequestDelegate next = _ => throw thrownException;
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, logger.Entries[0].Level);
    }

    [Fact]
    public async Task InvokeAsync_RethrowsOperationCanceledException_WhenRequestIsCancelled()
    {
        // Arrange
        OperationCanceledException expected = new("client disconnected");
        RequestDelegate next = _ => throw expected;
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();

        // Act
        OperationCanceledException actual = await Assert.ThrowsAsync<OperationCanceledException>(
            () => middleware.InvokeAsync(context));

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotLog_WhenRequestIsCancelled()
    {
        // Arrange
        RequestDelegate next = _ => throw new OperationCanceledException("client disconnected");
        CapturingLogger<GlobalExceptionMiddleware> logger = new();
        GlobalExceptionMiddleware middleware = new(next, logger);
        DefaultHttpContext context = new();

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => middleware.InvokeAsync(context));

        // Assert
        Assert.Empty(logger.Entries);
    }
}
