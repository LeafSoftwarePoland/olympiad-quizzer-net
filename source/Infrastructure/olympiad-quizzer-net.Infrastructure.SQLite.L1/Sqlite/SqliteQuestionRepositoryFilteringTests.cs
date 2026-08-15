using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class SqliteQuestionRepositoryFilteringTests : IDisposable
{
    private const string _categoryRecursion   = "rekurencja";
    private const string _categorySorting     = "sortowanie";
    private const string _categoryComplexity  = "zlozonosc";
    private const string _categoryCodeTracing = "sledzenie_kodu";
    private const string _categoryGraphsTrees = "grafy_drzewa";
    private const string _algorithmBubble     = "sortowanie_babelkowe";
    private const string _algorithmEuclid     = "algorytm_euklidesa";
    private const string _stageE1             = "E1";
    private const string _stageE2             = "E2";
    private const string _stageE3             = "E3";

    private readonly SqliteFixtureHarness _harness;
    private readonly SqliteQuestionRepository _repository;

    public SqliteQuestionRepositoryFilteringTests()
    {
        _harness    = new SqliteFixtureHarness("filtering-bank.json");
        _repository = RepositoryHarness.Repository(_harness);
    }

    public void Dispose() => _harness.Dispose();

    [Fact]
    public async Task GetAsync_WithNoFilters_ReturnsUpToLimitFromWholeBank()
    {
        QuestionQuery query = new();

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15],
            RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithOneCategory_ReturnsOnlyQuestionsCarryingThatCategory()
    {
        QuestionQuery query = new() { Categories = [_categoryRecursion] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 3, 11], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoCategories_ReturnsUnionOfBoth()
    {
        QuestionQuery query = new()
        {
            Categories = [_categoryRecursion, _categorySorting]
        };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 3, 4, 5, 11, 12, 13, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryNotPresentInBank_ReturnsEmptyList()
    {
        QuestionQuery query = new() { Categories = [_categoryGraphsTrees] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WithCategoryInDifferentCasing_DoesMatchCaseInsensitively()
    {
        QuestionQuery query = new() { Categories = ["REKURENCJA"] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 3, 11], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryWithSurroundingWhitespace_DoesMatch()
    {
        QuestionQuery query = new() { Categories = ["  rekurencja  "] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 3, 11], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithOneAlgorithm_ReturnsOnlyQuestionsCarryingIt()
    {
        QuestionQuery query = new() { Algorithms = [_algorithmBubble] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([4, 5, 7, 13, 14, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoAlgorithms_ReturnsUnionOfBoth()
    {
        QuestionQuery query = new()
        {
            Algorithms = [_algorithmBubble, _algorithmEuclid]
        };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([2, 4, 5, 7, 13, 14, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithAlgorithmOnQuestionThatHasEmptyAlgorithmsList_DoesExcludeIt()
    {
        QuestionQuery query = new() { Algorithms = [_algorithmEuclid] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([2, 7], RepositoryHarness.SortedIds(result));
        Assert.DoesNotContain(result, q => q.Id == 1);
    }

    [Fact]
    public async Task GetAsync_WithOneYear_ReturnsOnlyThatYear()
    {
        QuestionQuery query = new() { Years = [2024] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([3, 4, 10, 14, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoYears_ReturnsUnionOfBoth()
    {
        QuestionQuery query = new() { Years = [2023, 2025] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 5, 6, 7, 9, 11, 13], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithYearFilter_DoesExcludeQuestionsWithNullYear()
    {
        QuestionQuery query = new() { Years = [2023] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 9, 13], RepositoryHarness.SortedIds(result));
        Assert.DoesNotContain(result, q => q.Id == 8);
        Assert.DoesNotContain(result, q => q.Id == 12);
    }

    [Fact]
    public async Task GetAsync_WithOneStage_ReturnsOnlyThatStage()
    {
        QuestionQuery query = new() { Stages = [_stageE2] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([6, 12, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithTwoStages_ReturnsUnionOfBoth()
    {
        QuestionQuery query = new() { Stages = [_stageE2, _stageE3] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([6, 9, 12, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryAndYear_ReturnsOnlyQuestionsSatisfyingBoth()
    {
        QuestionQuery query = new()
        {
            Categories = [_categorySorting],
            Years = [2024]
        };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([3, 4, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithCategoryAndYearWhereNoQuestionSatisfiesBoth_ReturnsEmptyList()
    {
        QuestionQuery query = new()
        {
            Categories = [_categoryCodeTracing],
            Years = [2025]
        };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WithTwoCategoriesAndTwoYears_ReturnsOrWithinAndAndAcross()
    {
        QuestionQuery query = new()
        {
            Categories = [_categoryRecursion, _categoryComplexity],
            Years = [2023, 2024]
        };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([1, 2, 3, 10, 14], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithAllFourFilterTypes_DoesApplyEveryPredicate()
    {
        QuestionQuery query = new()
        {
            Categories = [_categorySorting],
            Algorithms = [_algorithmBubble],
            Years = [2024],
            Stages = [_stageE1]
        };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal([4], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_WithEmptyStringFilterValue_DoesIgnoreIt()
    {
        QuestionQuery query = new() { Categories = [""] };

        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        Assert.Equal(15, result.Count);
    }

    [Fact]
    public async Task GetAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        QuestionQuery query = new();
        CancellationToken cancelled = new(canceled: true);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.GetAsync(query, cancelled));
    }
}
