using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class SqliteQuestionRepositoryTests : IDisposable
{
    private const string _filteringBank = "filtering-bank.json";
    private const string _largeBank     = "large-bank.json";
    private const int    _seedA         = 42;
    private const int    _seedB         = 99;

    private readonly SqliteFixtureHarness _harness;
    private readonly SqliteQuestionRepository _repository;

    public SqliteQuestionRepositoryTests()
    {
        _harness    = new SqliteFixtureHarness(_filteringBank);
        _repository = RepositoryHarness.Repository(_harness);
    }

    public void Dispose() => _harness.Dispose();

    #region GetAsync — basic filtering

    [Fact]
    public async Task GetAsync_ReturnsAllQuestionsUpToLimit_WhenNoFiltersAreApplied()
    {
        // Arrange
        QuestionQuery query = new();

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15],
            RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyMatchingQuestions_WhenCategoryFilterIsApplied()
    {
        // Arrange
        const string category = "rekurencja";
        QuestionQuery query = new() { Categories = [category] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([1, 2, 3, 11], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsUnionOfBothCategories_WhenTwoCategoriesAreGiven()
    {
        // Arrange
        const string categoryA = "rekurencja";
        const string categoryB = "sortowanie";
        QuestionQuery query = new() { Categories = [categoryA, categoryB] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([1, 2, 3, 4, 5, 11, 12, 13, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsEmpty_WhenCategoryIsNotPresentInBank()
    {
        // Arrange
        const string absentCategory = "grafy_drzewa";
        QuestionQuery query = new() { Categories = [absentCategory] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_DoesMatchCaseInsensitively_WhenCategoryFilterIsUpperCase()
    {
        // Arrange
        QuestionQuery query = new() { Categories = ["REKURENCJA"] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([1, 2, 3, 11], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_DoesMatch_WhenCategoryFilterHasSurroundingWhitespace()
    {
        // Arrange
        QuestionQuery query = new() { Categories = ["  rekurencja  "] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([1, 2, 3, 11], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyMatchingQuestions_WhenAlgorithmFilterIsApplied()
    {
        // Arrange
        const string algorithm = "sortowanie_babelkowe";
        QuestionQuery query = new() { Algorithms = [algorithm] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([4, 5, 7, 13, 14, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyMatchingQuestions_WhenYearFilterIsApplied()
    {
        // Arrange
        const int year = 2024;
        QuestionQuery query = new() { Years = [year] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([3, 4, 10, 14, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_DoesExcludeQuestionsWithNullYear_WhenYearFilterIsApplied()
    {
        // Arrange
        const int year = 2023;
        QuestionQuery query = new() { Years = [year] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(result, q => q.Id == 8);
        Assert.DoesNotContain(result, q => q.Id == 12);
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyMatchingQuestions_WhenStageFilterIsApplied()
    {
        // Arrange
        const string stage = "E2";
        QuestionQuery query = new() { Stages = [stage] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([6, 12, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsQuestionsMatchingBoth_WhenCategoryAndYearAreGiven()
    {
        // Arrange
        const string category = "sortowanie";
        const int year = 2024;
        QuestionQuery query = new() { Categories = [category], Years = [year] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([3, 4, 15], RepositoryHarness.SortedIds(result));
    }

    [Fact]
    public async Task GetAsync_ReturnsEmpty_WhenNoCandidateSatisfiesBothCategoryAndYear()
    {
        // Arrange
        const string category = "sledzenie_kodu";
        const int year = 2025;
        QuestionQuery query = new() { Categories = [category], Years = [year] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_DoesIgnoreEmptyStringFilter_WhenCategoryContainsEmptyString()
    {
        // Arrange
        QuestionQuery query = new() { Categories = [""] };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.Count);
    }

    #endregion

    #region GetAsync — limit clamping

    [Fact]
    public async Task GetAsync_ReturnsExactlyLimit_WhenLimitIsBelowMatchCount()
    {
        // Arrange
        const int requestedLimit = 5;
        QuestionQuery query = new() { Limit = requestedLimit };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(requestedLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllMatches_WhenLimitExceedsMatchCount()
    {
        // Arrange
        const string category = "rekurencja";
        const int limitAboveMatchCount = 10;
        QuestionQuery query = new() { Categories = [category], Limit = limitAboveMatchCount };

        // Act
        IReadOnlyList<Question> result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsThirtyQuestions_WhenLimitExceedsMaxAndBankIsLarge()
    {
        // Arrange
        using SqliteFixtureHarness largeHarness = new(_largeBank);
        var repository = RepositoryHarness.Repository(largeHarness);
        const int limitAboveMax = 100;
        QuestionQuery query = new() { Limit = limitAboveMax };

        // Act
        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.MaxLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_UsesDefaultLimit_WhenLimitIsZero()
    {
        // Arrange
        using SqliteFixtureHarness largeHarness = new(_largeBank);
        var repository = RepositoryHarness.Repository(largeHarness);
        QuestionQuery query = new() { Limit = 0 };

        // Act
        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.DefaultLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_UsesDefaultLimit_WhenLimitIsNegative()
    {
        // Arrange
        using SqliteFixtureHarness largeHarness = new(_largeBank);
        var repository = RepositoryHarness.Repository(largeHarness);
        const int negativeLimit = -5;
        QuestionQuery query = new() { Limit = negativeLimit };

        // Act
        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.DefaultLimit, result.Count);
    }

    #endregion

    #region GetAsync — shuffle

    [Fact]
    public async Task GetAsync_ReturnsDeterministicOrder_WhenShufflerIsSeeded()
    {
        // Arrange
        var repository = RepositoryHarness.Repository(_harness, new SeededShuffler(_seedA));
        QuestionQuery query = new();

        // Act
        IReadOnlyList<Question> first  = await repository.GetAsync(query, CancellationToken.None);
        IReadOnlyList<Question> second = await repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(RepositoryHarness.IdsInOrder(first), RepositoryHarness.IdsInOrder(second));
    }

    [Fact]
    public async Task GetAsync_ReturnsDifferentOrder_WhenTwoDifferentSeedsAreUsed()
    {
        // Arrange
        using SqliteFixtureHarness harnessA = new(_filteringBank);
        using SqliteFixtureHarness harnessB = new(_filteringBank);
        var repositoryA = RepositoryHarness.Repository(harnessA, new SeededShuffler(_seedA));
        var repositoryB = RepositoryHarness.Repository(harnessB, new SeededShuffler(_seedB));
        QuestionQuery query = new();

        // Act
        IReadOnlyList<Question> resultA = await repositoryA.GetAsync(query, CancellationToken.None);
        IReadOnlyList<Question> resultB = await repositoryB.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.NotEqual(RepositoryHarness.IdsInOrder(resultA), RepositoryHarness.IdsInOrder(resultB));
    }

    [Fact]
    public async Task GetAsync_SelectionIsNotFirstNByBankOrder_WhenLimitIsLessThanBankSize()
    {
        // Arrange
        using SqliteFixtureHarness largeHarness = new(_largeBank);
        var repository = RepositoryHarness.Repository(largeHarness, new SeededShuffler(_seedA));
        const int smallLimit = 5;
        QuestionQuery query = new() { Limit = smallLimit };

        // Act
        IReadOnlyList<Question> result = await repository.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.NotEqual([101, 102, 103, 104, 105], RepositoryHarness.SortedIds(result));
    }

    #endregion

    #region GetAsync — cancellation

    [Fact]
    public async Task GetAsync_ThrowsOperationCanceledException_WhenTokenIsCancelled()
    {
        // Arrange
        QuestionQuery query = new();
        CancellationToken cancelled = new(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.GetAsync(query, cancelled));
    }

    #endregion

    #region GetFilterOptionsAsync

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsTotalCount_EqualToBankSize()
    {
        // Arrange
        const int expectedCount = 15;

        // Act
        FilterOptions options = await _repository.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedCount, options.TotalQuestions);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsAllCategoriesPresentInBank()
    {
        // Act
        FilterOptions options = await _repository.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(
            new[] { "rekurencja", "sledzenie_kodu", "sortowanie", "zlozonosc" },
            options.Categories.Select(c => c.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsStagesSortedAlphabetically()
    {
        // Act
        FilterOptions options = await _repository.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "E1", "E2", "E3" }, options.Stages.Select(s => s.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsYearsAsSortedStrings()
    {
        // Act
        FilterOptions options = await _repository.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "2023", "2024", "2025" }, options.Years.Select(y => y.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ThrowsOperationCanceledException_WhenTokenIsCancelled()
    {
        // Arrange
        CancellationToken cancelled = new(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _repository.GetFilterOptionsAsync(cancelled));
    }

    #endregion
}
