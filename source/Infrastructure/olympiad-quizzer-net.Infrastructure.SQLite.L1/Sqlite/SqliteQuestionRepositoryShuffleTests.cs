using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class SqliteQuestionRepositoryShuffleTests
{
    private const string _filteringBank = "filtering-bank.json";
    private const string _largeBank     = "large-bank.json";
    private const int    _seedA         = 42;
    private const int    _seedB         = 99;

    [Fact]
    public async Task GetAsync_WithSeededShuffler_ReturnsDeterministicOrder()
    {
        using SqliteFixtureHarness harness = new(_filteringBank);
        var repository = RepositoryHarness.Repository(harness, new SeededShuffler(_seedA));
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
        using SqliteFixtureHarness harnessA = new(_filteringBank);
        using SqliteFixtureHarness harnessB = new(_filteringBank);
        var repositoryA = RepositoryHarness.Repository(harnessA, new SeededShuffler(_seedA));
        var repositoryB = RepositoryHarness.Repository(harnessB, new SeededShuffler(_seedB));
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
        using SqliteFixtureHarness harness = new(_largeBank);
        var repository = RepositoryHarness.Repository(harness, new SeededShuffler(_seedA));
        QuestionQuery query = new() { Limit = 5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.NotEqual([101, 102, 103, 104, 105], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_CalledTwiceWithSameSeed_ReturnsSameOrderBothTimes()
    {
        using SqliteFixtureHarness harness = new(_filteringBank);
        var repository = RepositoryHarness.Repository(harness, new SeededShuffler(_seedA));
        QuestionQuery query = new();

        IReadOnlyList<Question> first  = await repository.GetAsync(query, CancellationToken.None);
        IReadOnlyList<Question> second = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(
            RepositoryHarness.IdsInOrder(first),
            RepositoryHarness.IdsInOrder(second));
    }
}
