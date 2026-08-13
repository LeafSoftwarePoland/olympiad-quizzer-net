using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;

namespace OlympiadQuizzer.Api.L1.Infrastructure;

[Trait("Tier", "L1")]
public sealed class JsonQuestionRepositoryLimitTests
{
    private const string FilteringBank = "filtering-bank.json";
    private const string LargeBank     = "large-bank.json";

    [Fact]
    public async Task GetAsync_WithLimitBelowMatchCount_ReturnsExactlyLimit()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Limit = 5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithLimitAboveMatchCount_ReturnsAllMatches()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Categories = new List<string> { "rekurencja" },
            Limit = 10
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithLimitAboveMaxLimit_CapsAtThirty()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(LargeBank);
        QuestionQuery query = new QuestionQuery { Limit = 100 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithZeroLimit_UsesDefaultLimit()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(LargeBank);
        QuestionQuery query = new QuestionQuery { Limit = 0 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithNegativeLimit_UsesDefaultLimit()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(LargeBank);
        QuestionQuery query = new QuestionQuery { Limit = -5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithNoFiltersOnBankLargerThanThirty_NeverReturnsMoreThanThirty()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(LargeBank);
        QuestionQuery query = new QuestionQuery();

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnedQuestionsAreDistinct_ReturnsNoDuplicatesWithinOneDraw()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(LargeBank);
        QuestionQuery query = new QuestionQuery();

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(30, result.Count);
        Assert.Equal(30, RepositoryHarness.SortedIds(result).Distinct().Count());
    }
}
