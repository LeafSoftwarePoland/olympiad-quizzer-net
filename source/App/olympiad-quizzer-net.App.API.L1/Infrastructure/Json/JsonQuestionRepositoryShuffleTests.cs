using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.App.Api.L1.Infrastructure.Json;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class JsonQuestionRepositoryShuffleTests
{
    private const string _filteringBank = "filtering-bank.json";
    private const string _largeBank     = "large-bank.json";
    private const int    _seedA         = 42;
    private const int    _seedB         = 99;

    [Fact]
    public async Task GetAsync_WithSeededShuffler_ReturnsDeterministicOrder()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(
            _filteringBank, new SeededShuffler(_seedA));
        QuestionQuery query = new();

        IReadOnlyList<Question> first  = await repository.GetAsync(query, CancellationToken.None);
        IReadOnlyList<Question> second = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(
            RepositoryHarness.IdsInOrder(first),
            RepositoryHarness.IdsInOrder(second));
    }

    [Fact]
    public async Task GetAsync_WithTwoDifferentSeeds_ReturnsDifferentOrder()
    {
        JsonQuestionRepository repositoryA = RepositoryHarness.Repository(
            _filteringBank, new SeededShuffler(_seedA));
        JsonQuestionRepository repositoryB = RepositoryHarness.Repository(
            _filteringBank, new SeededShuffler(_seedB));
        QuestionQuery query = new();

        IReadOnlyList<Question> resultA = await repositoryA.GetAsync(query, CancellationToken.None);
        IReadOnlyList<Question> resultB = await repositoryB.GetAsync(query, CancellationToken.None);

        Assert.NotEqual(
            RepositoryHarness.IdsInOrder(resultA),
            RepositoryHarness.IdsInOrder(resultB));
    }

    [Fact]
    public async Task GetAsync_WithLimitSmallerThanBank_SelectionIsNotTheFirstNInBankOrder()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(
            _largeBank, new SeededShuffler(_seedA));
        QuestionQuery query = new() { Limit = 5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.NotEqual([101, 102, 103, 104, 105], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_DoesNotMutateTheLoadedBankOrder()
    {
        (QuestionBankLoader loader, JsonQuestionRepository repository) = RepositoryHarness.Pair(
            _filteringBank, new SeededShuffler(_seedA));
        QuestionQuery query = new();

        await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(
            [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15],
            RepositoryHarness.IdsInOrder(loader.Bank.Questions));
    }
}
