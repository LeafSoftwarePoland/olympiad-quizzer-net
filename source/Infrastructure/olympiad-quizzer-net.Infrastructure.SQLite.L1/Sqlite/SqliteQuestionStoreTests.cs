using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class SqliteQuestionStoreTests : IDisposable
{
    private const string _filteringBank = "filtering-bank.json";

    private readonly SqliteFixtureHarness _harness;
    private readonly SqliteQuestionStore _store;

    public SqliteQuestionStoreTests()
    {
        _harness = new SqliteFixtureHarness(_filteringBank);
        _store   = BuildStore(_harness.DatabasePath);
    }

    public void Dispose() => _harness.Dispose();

    #region Constructor

    [Fact]
    public void Constructor_ThrowsFileNotFoundException_WhenDatabaseFileIsMissing()
    {
        // Arrange
        string missingPath = Path.Combine(
            Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N") + ".db");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => BuildStore(missingPath));
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenSchemaVersionDoesNotMatch()
    {
        // Arrange
        string dbPath = Path.ChangeExtension(Path.GetTempFileName(), ".db");
        try
        {
            using (SqliteConnection connection = new($"Data Source={dbPath}"))
            {
                connection.Open();
                connection.Execute("CREATE TABLE questions (id INTEGER PRIMARY KEY);");
                connection.Execute("PRAGMA user_version = 99;");
            }
            SqliteConnection.ClearAllPools();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => BuildStore(dbPath));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    #endregion

    #region SelectCandidates

    [Fact]
    public void SelectCandidates_ReturnsAllRows_WhenNoFiltersAreApplied()
    {
        // Arrange
        const int expectedCount = 15;

        // Act
        IReadOnlyList<QuestionCandidate> result = _store.SelectCandidates([], []);

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void SelectCandidates_ReturnsOnlyMatchingRows_WhenStageFilterIsApplied()
    {
        // Arrange
        const string stage = "E2";

        // Act
        IReadOnlyList<QuestionCandidate> result = _store.SelectCandidates([stage], []);

        // Assert
        Assert.Equal([6, 12, 15], result.Select(c => c.Id).Order().ToArray());
    }

    [Fact]
    public void SelectCandidates_ReturnsOnlyMatchingRows_WhenYearFilterIsApplied()
    {
        // Arrange
        const int year = 2024;

        // Act
        IReadOnlyList<QuestionCandidate> result = _store.SelectCandidates([], [year]);

        // Assert
        Assert.Equal([3, 4, 10, 14, 15], result.Select(c => c.Id).Order().ToArray());
    }

    [Fact]
    public void SelectCandidates_ReturnsOnlyMatchingRows_WhenBothStageAndYearFiltersAreApplied()
    {
        // Arrange
        const string stage = "E1";
        const int year = 2024;

        // Act
        IReadOnlyList<QuestionCandidate> result = _store.SelectCandidates([stage], [year]);

        // Assert
        Assert.Equal([3, 4, 10, 14], result.Select(c => c.Id).Order().ToArray());
    }

    [Fact]
    public void SelectCandidates_DoesPopulateCategoryAndAlgorithmsJson_ForReturnedRows()
    {
        // Arrange
        const int targetId = 1;

        // Act
        IReadOnlyList<QuestionCandidate> result = _store.SelectCandidates([], []);

        // Assert
        QuestionCandidate candidate = result.Single(c => c.Id == targetId);
        Assert.NotNull(candidate.Category);
        Assert.NotNull(candidate.Algorithms);
        Assert.Contains("rekurencja", candidate.Category);
    }

    #endregion

    #region FetchByIds

    [Fact]
    public void FetchByIds_ReturnsEmpty_WhenIdListIsEmpty()
    {
        // Act
        IReadOnlyList<QuestionRow> result = _store.FetchByIds([]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FetchByIds_ReturnsExactlyTheRequestedRows_WhenIdsAreGiven()
    {
        // Arrange
        int[] requestedIds = [1, 3, 5];

        // Act
        IReadOnlyList<QuestionRow> result = _store.FetchByIds(requestedIds);

        // Assert
        Assert.Equal(requestedIds, result.Select(r => r.Id).Order().ToArray());
    }

    [Fact]
    public void FetchByIds_DoesPopulateAllPascalCaseProperties_WhenRowIsReturned()
    {
        // Arrange
        int[] ids = [1];

        // Act
        IReadOnlyList<QuestionRow> result = _store.FetchByIds(ids);

        // Assert
        QuestionRow row = Assert.Single(result);
        Assert.Equal(1, row.Id);
        Assert.Equal("OIJ", row.Olympiad);
        Assert.Equal("E1", row.Stage);
        Assert.Equal(2023, row.Year);
        Assert.Equal("single", row.Type);
        Assert.NotNull(row.Content);
        Assert.NotNull(row.Category);
    }

    #endregion

    #region LoadSummary

    [Fact]
    public void LoadSummary_ReturnsTotalCount_EqualToBankSize()
    {
        // Arrange
        const int expectedCount = 15;

        // Act
        BankSummary summary = _store.LoadSummary();

        // Assert
        Assert.Equal(expectedCount, summary.TotalCount);
    }

    [Fact]
    public void LoadSummary_ReturnsStages_WithCorrectValuesAndCounts()
    {
        // Act
        BankSummary summary = _store.LoadSummary();

        // Assert
        Assert.Equal(["E1", "E2", "E3"], summary.Stages.Select(s => s.Value).ToArray());
        Assert.Equal([11, 3, 1], summary.Stages.Select(s => s.Count).ToArray());
    }

    [Fact]
    public void LoadSummary_ReturnsYears_AsSortedStrings()
    {
        // Act
        BankSummary summary = _store.LoadSummary();

        // Assert
        Assert.Equal(["2023", "2024", "2025"], summary.Years.Select(y => y.Value).ToArray());
    }

    [Fact]
    public void LoadSummary_ReturnsCategoryJsons_OnePerQuestion()
    {
        // Arrange
        const int expectedCount = 15;

        // Act
        BankSummary summary = _store.LoadSummary();

        // Assert
        Assert.Equal(expectedCount, summary.CategoryJsons.Count);
        Assert.All(summary.CategoryJsons, json => Assert.False(string.IsNullOrEmpty(json)));
    }

    [Fact]
    public void LoadSummary_ReturnsAlgorithmJsons_OnePerQuestion()
    {
        // Arrange
        const int expectedCount = 15;

        // Act
        BankSummary summary = _store.LoadSummary();

        // Assert
        Assert.Equal(expectedCount, summary.AlgorithmJsons.Count);
    }

    #endregion

    private static SqliteQuestionStore BuildStore(string databasePath)
    {
        var options = Options.Create(new QuestionBankOptions { DatabasePath = databasePath });
        return new SqliteQuestionStore(options);
    }
}
