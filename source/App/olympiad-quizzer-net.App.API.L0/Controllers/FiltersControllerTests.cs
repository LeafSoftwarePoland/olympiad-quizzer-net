using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using OlympiadQuizzer.App.Api.Controllers;
using OlympiadQuizzer.App.Api.Middleware;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Errors;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using System.Text.Json;

namespace OlympiadQuizzer.App.Api.L0.Controllers;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class FiltersControllerTests
{
    private static FiltersController BuildController(
        Mock<IQuestionRepository> mockRepository,
        CapturingLogger<FiltersController> logger,
        string traceId = "trace-id")
    {
        FiltersController controller = new(mockRepository.Object, logger);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.TraceIdentifier = traceId;
        return controller;
    }

    [Fact]
    public async Task Get_ReturnsOkWithFilterOptions_WhenRepositoryReturnsData()
    {
        // Arrange
        FilterOptions options = new();
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger);

        // Act
        IActionResult result = await controller.Get(CancellationToken.None);

        // Assert
        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(options, ok.Value);
    }

    [Fact]
    public async Task Get_Returns500WithUnexpectedCode_WhenRepositoryThrowsSqliteException()
    {
        // Arrange
        const string traceId = "trace-sqlite-filters";
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SqliteException("database error", 1));
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger, traceId);

        // Act
        IActionResult result = await controller.Get(CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        ErrorResponse body = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.Unexpected, body.Code);
        Assert.Equal(traceId, body.RequestId);
        Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Get_Returns500WithUnexpectedCode_WhenRepositoryThrowsIOException()
    {
        // Arrange
        const string traceId = "trace-io-filters";
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("disk error"));
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger, traceId);

        // Act
        IActionResult result = await controller.Get(CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        ErrorResponse body = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.Unexpected, body.Code);
        Assert.Equal(traceId, body.RequestId);
        Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Get_Returns500WithUnexpectedCode_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        const string traceId = "trace-inv-op-filters";
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("schema mismatch"));
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger, traceId);

        // Act
        IActionResult result = await controller.Get(CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        ErrorResponse body = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.Unexpected, body.Code);
        Assert.Equal(traceId, body.RequestId);
        Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Get_Returns500WithUnexpectedCode_WhenRepositoryThrowsJsonException()
    {
        // Arrange
        const string traceId = "trace-json-filters";
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new JsonException("bad json"));
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger, traceId);

        // Act
        IActionResult result = await controller.Get(CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        ErrorResponse body = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.Unexpected, body.Code);
        Assert.Equal(traceId, body.RequestId);
        Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Get_BubblesException_WhenRepositoryThrowsUnanticipated()
    {
        // Arrange
        DivideByZeroException expected = new("unexpected");
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger);

        // Act
        DivideByZeroException actual = await Assert.ThrowsAsync<DivideByZeroException>(
            () => controller.Get(CancellationToken.None));

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task Get_BubblesException_WhenRepositoryThrowsOperationCanceledException()
    {
        // Arrange
        OperationCanceledException expected = new("cancelled");
        Mock<IQuestionRepository> mockRepository = new();
        mockRepository
            .Setup(r => r.GetFilterOptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);
        CapturingLogger<FiltersController> logger = new();
        FiltersController controller = BuildController(mockRepository, logger);

        // Act
        OperationCanceledException actual = await Assert.ThrowsAsync<OperationCanceledException>(
            () => controller.Get(CancellationToken.None));

        // Assert
        Assert.Same(expected, actual);
    }
}
