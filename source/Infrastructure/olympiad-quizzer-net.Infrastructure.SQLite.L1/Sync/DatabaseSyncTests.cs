using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Sync;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sync;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class DatabaseSyncTests
{
    private const string _singleBank   = "single-question-bank.json";
    private const string _filteringBank = "filtering-bank.json";
    #region Check

    [Fact]
    public void Check_ReturnsEmptyDelta_WhenProductionBankAndDatabaseAreInSync()
    {
        // Arrange
        string repoRoot = FixturePath.RepoRoot();
        string jsonPath = Path.Combine(repoRoot, "data", "questions.json");
        string dbPath   = Path.Combine(repoRoot, "data", "questions.db");

        // Act
        SyncDelta delta = DatabaseSync.Check(jsonPath, dbPath);

        // Assert
        Assert.True(
            delta.IsEmpty,
            $"Bank and database are out of sync:{Environment.NewLine}{delta.FormatReport()}");
    }

    [Fact]
    public void Check_ReturnsEmptyDelta_WhenFixtureIsAlreadySynced()
    {
        // Arrange
        using SqliteFixtureHarness harness = new(_filteringBank);
        string jsonPath = FixturePath.Resolve(_filteringBank);

        // Act
        SyncDelta delta = DatabaseSync.Check(jsonPath, harness.DatabasePath);

        // Assert
        Assert.True(delta.IsEmpty, $"Expected empty delta but got:{Environment.NewLine}{delta.FormatReport()}");
    }

    [Fact]
    public void Check_ThrowsFileNotFoundException_WhenDatabaseIsMissing()
    {
        // Arrange
        string repoRoot  = FixturePath.RepoRoot();
        string jsonPath  = Path.Combine(repoRoot, "data", "questions.json");
        string missingDb = Path.Combine(Path.GetTempPath(),
            "does-not-exist-" + Guid.NewGuid().ToString("N") + ".db");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => DatabaseSync.Check(jsonPath, missingDb));
    }
    #endregion

    #region Sync — delta reporting

    [Fact]
    public void Sync_ReportsAllQuestionsAsAdded_WhenDatabaseIsNewlyCreated()
    {
        // Arrange — sync from a single-question fixture to a brand-new DB path
        string jsonPath = FixturePath.Resolve(_singleBank);
        string dbPath   = Path.ChangeExtension(Path.GetTempFileName(), ".db");

        try
        {
            // Act — first-ever sync: DB does not exist yet
            SyncDelta delta = DatabaseSync.Sync(jsonPath, dbPath);

            // Assert
            Assert.Equal([1], delta.Added.ToArray());
            Assert.Empty(delta.Changed);
            Assert.Empty(delta.Removed);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void Sync_ReturnsEmptyDelta_WhenResyncedWithUnchangedJson()
    {
        // Arrange — harness already did the first sync
        using SqliteFixtureHarness harness = new(_singleBank);
        string jsonPath = FixturePath.Resolve(_singleBank);

        // Act — re-sync the same JSON
        SyncDelta delta = DatabaseSync.Sync(jsonPath, harness.DatabasePath);

        // Assert — nothing changed
        Assert.True(delta.IsEmpty, $"Expected empty delta on re-sync but got:{Environment.NewLine}{delta.FormatReport()}");
    }

    [Fact]
    public void Check_ReportsRemovedId_WhenQuestionAbsentFromJson()
    {
        // Arrange — DB has one question (id=1); present an empty JSON to Check
        using SqliteFixtureHarness harness = new(_singleBank);
        string tempJsonPath = WriteTempJson("[]");

        try
        {
            // Act
            SyncDelta delta = DatabaseSync.Check(tempJsonPath, harness.DatabasePath);

            // Assert
            Assert.Equal([1], delta.Removed.ToArray());
            Assert.Empty(delta.Added);
            Assert.Empty(delta.Changed);
        }
        finally
        {
            if (File.Exists(tempJsonPath)) File.Delete(tempJsonPath);
        }
    }

    [Fact]
    public void Check_ReportsChangedId_WhenQuestionContentHashDiffers()
    {
        // Arrange — sync the fixture, then modify one field and re-check
        using SqliteFixtureHarness harness = new(_singleBank);
        string originalJsonPath = FixturePath.Resolve(_singleBank);

        // Load and modify the question's source field (affects content hash)
        List<Question> questions = LoadQuestions(originalJsonPath);
        questions[0].Source += "_modified";
        string modifiedJsonPath = WriteTempJson(JsonSerializer.Serialize(questions, JsonOptions.Default));

        try
        {
            // Act
            SyncDelta delta = DatabaseSync.Check(modifiedJsonPath, harness.DatabasePath);

            // Assert
            Assert.Equal([1], delta.Changed.ToArray());
            Assert.Empty(delta.Added);
            Assert.Empty(delta.Removed);
        }
        finally
        {
            if (File.Exists(modifiedJsonPath)) File.Delete(modifiedJsonPath);
        }
    }

    [Fact]
    public void Sync_MakesSubsequentCheckReturnEmptyDelta_AfterApplyingChangedJson()
    {
        // Arrange — start from fixture, modify the question, sync the change, then verify Check is clean
        using SqliteFixtureHarness harness = new(_singleBank);
        string originalJsonPath = FixturePath.Resolve(_singleBank);

        List<Question> questions = LoadQuestions(originalJsonPath);
        questions[0].Source += "_v2";
        string modifiedJsonPath = WriteTempJson(JsonSerializer.Serialize(questions, JsonOptions.Default));

        try
        {
            // Act — sync the changed JSON to the existing DB
            DatabaseSync.Sync(modifiedJsonPath, harness.DatabasePath);

            // Assert — Check with the same modified JSON should now return empty delta
            SyncDelta delta = DatabaseSync.Check(modifiedJsonPath, harness.DatabasePath);
            Assert.True(delta.IsEmpty,
                $"Expected empty delta after sync but got:{Environment.NewLine}{delta.FormatReport()}");
        }
        finally
        {
            if (File.Exists(modifiedJsonPath)) File.Delete(modifiedJsonPath);
        }
    }
    #endregion

    #region Helpers

    private static string WriteTempJson(string json)
    {
        string path = Path.ChangeExtension(Path.GetTempFileName(), ".json");
        File.WriteAllText(path, json, Encoding.UTF8);
        return path;
    }

    private static List<Question> LoadQuestions(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath, Encoding.UTF8);
        return JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException($"Could not deserialize {jsonPath}.");
    }

    #endregion
}
