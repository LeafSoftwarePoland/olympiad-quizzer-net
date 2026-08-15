using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class SqliteQuestionRepositoryLimitTests
{
    private const string _filteringBank = "filtering-bank.json";
    private const string _largeBank     = "large-bank.json";

    [Fact]
    public async Task GetAsync_WithLimitBelowMatchCount_ReturnsExactlyLimit()
    {
        using SqliteFixtureHarness harness = new(_filteringBank);
        QuestionQuery query = new() { Limit = 5 };

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithLimitAboveMatchCount_ReturnsAllMatches()
    {
        using SqliteFixtureHarness harness = new(_filteringBank);
        QuestionQuery query = new()
        {
            Categories = ["rekurencja"],
            Limit = 10
        };

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithLimitAboveMaxLimit_ReturnsThirtyQuestions()
    {
        using SqliteFixtureHarness harness = new(_largeBank);
        QuestionQuery query = new() { Limit = 100 };

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithZeroLimit_DoesUseDefaultLimit()
    {
        using SqliteFixtureHarness harness = new(_largeBank);
        QuestionQuery query = new() { Limit = 0 };

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithNegativeLimit_DoesUseDefaultLimit()
    {
        using SqliteFixtureHarness harness = new(_largeBank);
        QuestionQuery query = new() { Limit = -5 };

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithNoFiltersOnBankLargerThanThirty_NeverReturnsMoreThanThirty()
    {
        using SqliteFixtureHarness harness = new(_largeBank);
        QuestionQuery query = new();

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnedQuestionsAreDistinct_ReturnsNoDuplicatesWithinOneDraw()
    {
        using SqliteFixtureHarness harness = new(_largeBank);
        QuestionQuery query = new();

        IReadOnlyList<Question> result = await RepositoryHarness.Repository(harness)
            .GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
        Assert.Equal(30, RepositoryHarness.SortedIds(result).Distinct().Count());
    }
}
