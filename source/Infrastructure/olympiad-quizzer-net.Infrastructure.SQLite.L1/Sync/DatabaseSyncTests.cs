using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Sync;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Sync;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class DatabaseSyncTests
{
    [Fact]
    public void Check_ProductionBankAndDatabase_ReturnsEmptyDelta()
    {
        string repoRoot = FixturePath.RepoRoot();
        string jsonPath = Path.Combine(repoRoot, "data", "questions.json");
        string dbPath   = Path.Combine(repoRoot, "data", "questions.db");

        SyncDelta delta = DatabaseSync.Check(jsonPath, dbPath);

        Assert.True(
            delta.IsEmpty,
            $"Bank and database are out of sync:{Environment.NewLine}{delta.FormatReport()}");
    }

    [Fact]
    public void Check_AlreadySyncedFixture_ReturnsEmptyDelta()
    {
        using SqliteFixtureHarness harness = new("filtering-bank.json");
        string jsonPath = FixturePath.Resolve("filtering-bank.json");

        SyncDelta delta = DatabaseSync.Check(jsonPath, harness.DatabasePath);

        Assert.True(delta.IsEmpty, $"Expected empty delta but got:{Environment.NewLine}{delta.FormatReport()}");
    }

    [Fact]
    public void Check_WhenDatabaseIsMissing_ThrowsFileNotFoundException()
    {
        string repoRoot = FixturePath.RepoRoot();
        string jsonPath = Path.Combine(repoRoot, "data", "questions.json");
        string missingDb = Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N") + ".db");

        Assert.Throws<FileNotFoundException>(() => DatabaseSync.Check(jsonPath, missingDb));
    }
}
