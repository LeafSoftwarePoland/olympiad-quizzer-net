using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;

namespace OlympiadQuizzer.Api.L1.Infrastructure;

[Trait("Tier", "L1")]
public sealed class JsonQuestionRepositoryFilteringTests
{
    private const string FilteringBank        = "filtering-bank.json";
    private const string CategoryRecursion    = "rekurencja";
    private const string CategorySorting      = "sortowanie";
    private const string CategoryComplexity   = "zlozonosc";
    private const string CategoryCodeTracing  = "sledzenie_kodu";
    private const string CategoryGraphsTrees  = "grafy_drzewa";
    private const string AlgorithmBubble      = "sortowanie_babelkowe";
    private const string AlgorithmEuclid      = "algorytm_euklidesa";
    private const string StageE1              = "E1";
    private const string StageE2              = "E2";
    private const string StageE3              = "E3";

    [Fact]
    public async Task GetAsync_WithNoFilters_ReturnsUpToLimitFromWholeBank()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery();

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithOneCategory_ReturnsOnlyQuestionsCarryingThatCategory()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Categories = new List<string> { CategoryRecursion } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3, 11 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoCategories_ReturnsUnionOfBoth()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Categories = new List<string> { CategoryRecursion, CategorySorting }
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 11, 12, 13, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryNotPresentInBank_ReturnsEmptyList()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Categories = new List<string> { CategoryGraphsTrees } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WithCategoryInDifferentCasing_MatchesCaseInsensitively()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Categories = new List<string> { "REKURENCJA" } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3, 11 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryWithSurroundingWhitespace_Matches()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Categories = new List<string> { "  rekurencja  " } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3, 11 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithOneAlgorithm_ReturnsOnlyQuestionsCarryingIt()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Algorithms = new List<string> { AlgorithmBubble } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 4, 5, 7, 13, 14, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoAlgorithms_ReturnsUnionOfBoth()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Algorithms = new List<string> { AlgorithmBubble, AlgorithmEuclid }
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 2, 4, 5, 7, 13, 14, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithAlgorithmOnQuestionThatHasEmptyAlgorithmsList_ExcludesIt()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Algorithms = new List<string> { AlgorithmEuclid } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 2, 7 }, RepositoryHarness.SortedIds(result));
        Assert.DoesNotContain(result, q => q.Id == 1);
    }

    [Fact]
    public async Task GetAsync_WithOneYear_ReturnsOnlyThatYear()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Years = new List<int> { 2024 } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 3, 4, 10, 14, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoYears_ReturnsUnionOfBoth()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Years = new List<int> { 2023, 2025 } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 5, 6, 7, 9, 11, 13 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithYearFilter_ExcludesQuestionsWithNullYear()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Years = new List<int> { 2023 } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 9, 13 }, RepositoryHarness.SortedIds(result));
        Assert.DoesNotContain(result, q => q.Id == 8);
        Assert.DoesNotContain(result, q => q.Id == 12);
    }

    [Fact]
    public async Task GetAsync_WithOneStage_ReturnsOnlyThatStage()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Stages = new List<string> { StageE2 } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 6, 12, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoStages_ReturnsUnionOfBoth()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Stages = new List<string> { StageE2, StageE3 } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 6, 9, 12, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryAndYear_ReturnsOnlyQuestionsSatisfyingBoth()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Categories = new List<string> { CategorySorting },
            Years = new List<int> { 2024 }
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 3, 4, 15 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryAndYearWhereNoQuestionSatisfiesBoth_ReturnsEmptyList()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Categories = new List<string> { CategoryCodeTracing },
            Years = new List<int> { 2025 }
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WithTwoCategoriesAndTwoYears_ReturnsOrWithinAndAndAcross()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Categories = new List<string> { CategoryRecursion, CategoryComplexity },
            Years = new List<int> { 2023, 2024 }
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 1, 2, 3, 10, 14 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithAllFourFilterTypes_AppliesEveryPredicate()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery
        {
            Categories = new List<string> { CategorySorting },
            Algorithms = new List<string> { AlgorithmBubble },
            Years = new List<int> { 2024 },
            Stages = new List<string> { StageE1 }
        };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 4 }, RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithEmptyStringFilterValue_IgnoresIt()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery { Categories = new List<string> { "" } };

        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(15, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);
        QuestionQuery query = new QuestionQuery();
        CancellationToken cancelled = new CancellationToken(canceled: true);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repository.GetAsync(query, cancelled));
    }
}
