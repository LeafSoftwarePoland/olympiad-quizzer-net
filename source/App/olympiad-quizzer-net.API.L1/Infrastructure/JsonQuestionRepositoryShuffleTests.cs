using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Api.L1.Infrastructure;

[Trait("Tier", "L1")]
public sealed class JsonQuestionRepositoryShuffleTests
{
    private const string FilteringBank = "filtering-bank.json";
    private const string LargeBank     = "large-bank.json";
    private const int    SeedA         = 42;
    private const int    SeedB         = 99;

    [Fact]
    public async Task GetAsync_WithSeededShuffler_ReturnsDeterministicOrder()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(
            FilteringBank, new SeededShuffler(SeedA));
        QuestionQuery query = new QuestionQuery();

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
            FilteringBank, new SeededShuffler(SeedA));
        JsonQuestionRepository repositoryB = RepositoryHarness.Repository(
            FilteringBank, new SeededShuffler(SeedB));
        QuestionQuery query = new QuestionQuery();

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
            LargeBank, new SeededShuffler(SeedA));
        QuestionQuery query = new QuestionQuery { Limit = 5 };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.NotEqual(new[] { 101, 102, 103, 104, 105 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_DoesNotMutateTheLoadedBankOrder()
    {
        (QuestionBankLoader loader, JsonQuestionRepository repository) = RepositoryHarness.Pair(
            FilteringBank, new SeededShuffler(SeedA));
        QuestionQuery query = new QuestionQuery();

        await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(
            new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            RepositoryHarness.IdsInOrder(loader.Bank.Questions));
    }
}
