using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class SqliteQuestionRepositoryFilterOptionsTests : IDisposable
{
    private const string _filteringBank = "filtering-bank.json";
    private const string _singleBank    = "single-question-bank.json";

    private readonly SqliteFixtureHarness _harness;

    public SqliteQuestionRepositoryFilterOptionsTests()
    {
        _harness = new SqliteFixtureHarness(_filteringBank);
    }

    public void Dispose() => _harness.Dispose();

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsEveryCategoryPresentInBank()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(
            new[] { "rekurencja", "sledzenie_kodu", "sortowanie", "zlozonosc" },
            options.Categories.Select(c => c.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_DoesNotReturnCategoriesAbsentFromBank()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.DoesNotContain(options.Categories, c => c.Value == "grafy_drzewa");
    }

    [Fact]
    public async Task GetFilterOptionsAsync_CategoryCounts_DoesMatchQuestionCounts()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal([4, 3, 6, 4], [.. options.Categories.Select(c => c.Count)]);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_YearField_DoesExcludeNullYears()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(3, options.Years.Count);
        Assert.Equal(13, options.Years.Sum(y => y.Count));
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsYearsAsStringsSortedAscending()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { "2023", "2024", "2025" }, options.Years.Select(y => y.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsStagesPresentInBank()
    {
        using SqliteFixtureHarness singleHarness = new(_singleBank);
        var repository = RepositoryHarness.Repository(singleHarness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { "E1" }, options.Stages.Select(s => s.Value).ToArray());
        Assert.DoesNotContain(options.Stages, s => s.Value == "E2");
        Assert.DoesNotContain(options.Stages, s => s.Value == "E3");
    }

    [Fact]
    public async Task GetFilterOptionsAsync_TotalQuestions_IsEqualToBankSize()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(15, options.TotalQuestions);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_OnEmptyAlgorithmsAcrossBank_ReturnsEmptyAlgorithmList()
    {
        using SqliteFixtureHarness singleHarness = new(_singleBank);
        var repository = RepositoryHarness.Repository(singleHarness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Empty(options.Algorithms);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_StageCounts_DoesMatchQuestionCounts()
    {
        var repository = RepositoryHarness.Repository(_harness);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { "E1", "E2", "E3" }, options.Stages.Select(s => s.Value).ToArray());
        Assert.Equal([11, 3, 1], [.. options.Stages.Select(s => s.Count)]);
    }
}
