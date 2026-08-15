using Microsoft.Data.Sqlite;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Sync;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

internal sealed class SqliteFixtureHarness : IDisposable
{
    private readonly string _dbPath;

    internal SqliteFixtureHarness(string fixtureName)
    {
        string jsonPath = FixturePath.Resolve(fixtureName);
        _dbPath = Path.ChangeExtension(Path.GetTempFileName(), ".db");
        DatabaseSync.Sync(jsonPath, _dbPath);
    }

    internal string DatabasePath => _dbPath;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
