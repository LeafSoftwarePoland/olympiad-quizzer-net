using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Infrastructure.SQLite.Json;

namespace OlympiadQuizzer.Api.L1.Infrastructure;

[Trait("Tier", "L1")]
public sealed class FilterOptionsTests
{
    private const string FilteringBank = "filtering-bank.json";
    private const string SingleBank    = "single-question-bank.json";

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsEveryCategoryPresentInBank()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(
            new[] { "rekurencja", "sledzenie_kodu", "sortowanie", "zlozonosc" },
            options.Categories.Select(c => c.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_DoesNotReturnCategoriesAbsentFromBank()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.DoesNotContain(options.Categories, c => c.Value == "grafy_drzewa");
    }

    [Fact]
    public async Task GetFilterOptionsAsync_CategoryCountsMatchQuestionCounts()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { 4, 3, 6, 4 }, options.Categories.Select(c => c.Count).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ExcludesNullYears()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(3, options.Years.Count);
        Assert.Equal(13, options.Years.Sum(y => y.Count));
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsYearsAsStringsSortedAscending()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { "2023", "2024", "2025" }, options.Years.Select(y => y.Value).ToArray());
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsStagesPresentInBank()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(SingleBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { "E1" }, options.Stages.Select(s => s.Value).ToArray());
        Assert.DoesNotContain(options.Stages, s => s.Value == "E2");
        Assert.DoesNotContain(options.Stages, s => s.Value == "E3");
    }

    [Fact]
    public async Task GetFilterOptionsAsync_TotalQuestionsMatchesBankSize()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(15, options.TotalQuestions);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_OnEmptyAlgorithmsAcrossBank_ReturnsEmptyAlgorithmList()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(SingleBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Empty(options.Algorithms);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_StageCountsMatchQuestionCounts()
    {
        JsonQuestionRepository repository = RepositoryHarness.Repository(FilteringBank);

        FilterOptions options = await repository.GetFilterOptionsAsync(CancellationToken.None);

        Assert.Equal(new[] { "E1", "E2", "E3" }, options.Stages.Select(s => s.Value).ToArray());
        Assert.Equal(new[] { 11, 3, 1 }, options.Stages.Select(s => s.Count).ToArray());
    }
}
