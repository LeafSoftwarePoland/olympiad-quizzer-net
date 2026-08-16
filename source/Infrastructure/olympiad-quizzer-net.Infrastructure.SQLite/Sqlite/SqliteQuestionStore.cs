using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Domain.Queries;

namespace OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

public sealed class SqliteQuestionStore : IQuestionStore
{
    private const int SchemaVersion = 1;

    private const string FetchSql = @"
        SELECT
            id                 AS Id,
            olympiad           AS Olympiad,
            stage              AS Stage,
            year               AS Year,
            difficulty         AS Difficulty,
            source             AS Source,
            source_url         AS SourceUrl,
            source_raw         AS SourceRaw,
            explanation_source AS ExplanationSource,
            type               AS Type,
            content            AS Content,
            content_cpp        AS ContentCpp,
            options            AS Options,
            match_options      AS MatchOptions,
            correct_answer     AS CorrectAnswer,
            category           AS Category,
            algorithms         AS Algorithms,
            explanation        AS Explanation,
            points             AS Points,
            partial_credit     AS PartialCredit,
            content_hash       AS ContentHash
        FROM questions
        WHERE id IN @Ids";

    private readonly string _databasePath;

    public SqliteQuestionStore(IOptions<QuestionBankOptions> options)
    {
        string configured = options.Value.DatabasePath;
        string path = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Question database not found: {path}");
        }

        using SqliteConnection connection = new($"Data Source={path};Mode=ReadOnly");
        connection.Open();
        long version = connection.ExecuteScalar<long>("PRAGMA user_version;");
        if (version != SchemaVersion)
        {
            throw new InvalidOperationException(
                $"Database schema version mismatch: expected {SchemaVersion}, found {version}.");
        }

        _databasePath = path;
    }

    public IReadOnlyList<QuestionCandidate> SelectCandidates(
        IReadOnlyCollection<string> stages, IReadOnlyCollection<int> years)
    {
        using SqliteConnection connection = new($"Data Source={_databasePath};Mode=ReadOnly");
        connection.Open();

        StringBuilder sql = new("SELECT id, category, algorithms FROM questions");
        DynamicParameters parameters = new();
        List<string> conditions = [];

        if (stages.Count > 0)
        {
            conditions.Add("stage IN @Stages");
            parameters.Add("Stages", stages);
        }

        if (years.Count > 0)
        {
            conditions.Add("year IN @Years");
            parameters.Add("Years", years);
        }

        if (conditions.Count > 0)
        {
            sql.Append(" WHERE ");
            sql.Append(string.Join(" AND ", conditions));
        }

        return [.. connection.Query<QuestionCandidate>(sql.ToString(), parameters)];
    }

    public IReadOnlyList<QuestionRow> FetchByIds(IReadOnlyList<int> ids)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        using SqliteConnection connection = new($"Data Source={_databasePath};Mode=ReadOnly");
        connection.Open();

        return [.. connection.Query<QuestionRow>(FetchSql, new { Ids = ids })];
    }

    public BankSummary LoadSummary()
    {
        using SqliteConnection connection = new($"Data Source={_databasePath};Mode=ReadOnly");
        connection.Open();

        int totalCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM questions;");

        List<FilterOption> stages = [.. connection.Query<FilterOption>(
            "SELECT stage AS Value, COUNT(*) AS Count FROM questions GROUP BY stage ORDER BY stage;")];

        List<FilterOption> years = [.. connection.Query<FilterOption>(
            "SELECT CAST(year AS TEXT) AS Value, COUNT(*) AS Count " +
            "FROM questions WHERE year IS NOT NULL GROUP BY year ORDER BY year;")];

        List<string> categoryJsons = [.. connection.Query<string>("SELECT category FROM questions;")];

        List<string> algorithmJsons = [.. connection.Query<string>("SELECT algorithms FROM questions;")];

        return new BankSummary
        {
            TotalCount = totalCount,
            Stages = stages,
            Years = years,
            CategoryJsons = categoryJsons,
            AlgorithmJsons = algorithmJsons
        };
    }
}
