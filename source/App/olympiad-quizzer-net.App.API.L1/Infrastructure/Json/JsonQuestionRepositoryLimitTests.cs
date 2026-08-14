using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;

namespace OlympiadQuizzer.App.Api.L1.Infrastructure.Json;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class JsonQuestionRepositoryLimitTests
{
    private const string _filteringBank = "filtering-bank.json";
    private const string _largeBank     = "large-bank.json";

    [Fact]
    public async Task GetAsync_WithLimitBelowMatchCount_ReturnsExactlyLimit()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_filteringBank);
        QuestionQuery query = new() { Limit = 5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithLimitAboveMatchCount_ReturnsAllMatches()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_filteringBank);
        QuestionQuery query = new()
		{
            Categories = ["rekurencja"],
            Limit = 10
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithLimitAboveMaxLimit_ReturnsThirtyQuestions()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_largeBank);
        QuestionQuery query = new() { Limit = 100 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithZeroLimit_DoesUseDefaultLimit()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_largeBank);
        QuestionQuery query = new() { Limit = 0 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithNegativeLimit_DoesUseDefaultLimit()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_largeBank);
        QuestionQuery query = new() { Limit = -5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithNoFiltersOnBankLargerThanThirty_NeverReturnsMoreThanThirty()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_largeBank);
        QuestionQuery query = new();

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnedQuestionsAreDistinct_ReturnsNoDuplicatesWithinOneDraw()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(_largeBank);
        QuestionQuery query = new();

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
        Assert.Equal(30, RepositoryHarness.SortedIds(result).Distinct().Count());
    }
}
