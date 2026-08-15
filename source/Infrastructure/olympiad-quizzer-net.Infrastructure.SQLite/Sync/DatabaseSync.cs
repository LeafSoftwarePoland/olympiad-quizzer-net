using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Infrastructure.SQLite.Json;

namespace OlympiadQuizzer.Infrastructure.SQLite.Sync;

public static class DatabaseSync
{
    private const int SchemaVersion = 1;

    private static readonly JsonSerializerOptions _hashOptions = new(JsonOptions.Default)
    {
        WriteIndented = false
    };

    public static SyncDelta Sync(string jsonPath, string dbPath)
    {
        List<Question> questions = LoadJson(jsonPath);
        EnsureDatabase(dbPath);
        Dictionary<int, string> existingHashes = ReadHashes(dbPath);
        SyncDelta delta = ComputeDelta(questions, existingHashes);
        ApplyDelta(dbPath, questions, delta);
        return delta;
    }

    public static SyncDelta Check(string jsonPath, string dbPath)
    {
        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException($"Database not found at {dbPath}.");
        }
        List<Question> questions = LoadJson(jsonPath);
        Dictionary<int, string> existingHashes = ReadHashes(dbPath);
        return ComputeDelta(questions, existingHashes);
    }

    private static List<Question> LoadJson(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath, Encoding.UTF8);
        return JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default)
            ?? throw new InvalidOperationException($"Failed to deserialize questions from {jsonPath}.");
    }

    private static void EnsureDatabase(string dbPath)
    {
        string dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        bool exists = File.Exists(dbPath);

        using SqliteConnection connection = new($"Data Source={dbPath}");
        connection.Open();

        if (!exists)
        {
            string schemaPath = FindSchemaFile();
            string schemaSql = File.ReadAllText(schemaPath, Encoding.UTF8);
            connection.Execute(schemaSql);
        }

        connection.Execute($"PRAGMA user_version = {SchemaVersion};");
    }

    private static string FindSchemaFile()
    {
        string dir = AppContext.BaseDirectory;
        for (int depth = 0; depth < 10; depth++)
        {
            string candidate = Path.Combine(dir, "data", "schema.sql");
            if (File.Exists(candidate))
            {
                return candidate;
            }
            string slnCandidate = Path.Combine(dir, "OlympiadQuizzer.slnx");
            if (File.Exists(slnCandidate))
            {
                return Path.Combine(dir, "data", "schema.sql");
            }
            string parentDir = Path.GetDirectoryName(dir);
            if (parentDir == null)
            {
                break;
            }
            dir = parentDir;
        }
        throw new FileNotFoundException("Could not locate data/schema.sql relative to AppContext.BaseDirectory.");
    }

    private static Dictionary<int, string> ReadHashes(string dbPath)
    {
        using SqliteConnection connection = new($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();
        IEnumerable<(int Id, string Hash)> rows = connection.Query<(int Id, string Hash)>(
            "SELECT id, content_hash FROM questions;");
        Dictionary<int, string> result = [];
        foreach ((int id, string hash) in rows)
        {
            result[id] = hash;
        }
        return result;
    }

    private static SyncDelta ComputeDelta(List<Question> questions, Dictionary<int, string> existingHashes)
    {
        SyncDelta delta = new();
        HashSet<int> jsonIds = [];

        foreach (Question question in questions)
        {
            jsonIds.Add(question.Id);
            string newHash = ComputeHash(question);

            if (!existingHashes.TryGetValue(question.Id, out string existingHash))
            {
                delta.Added.Add(question.Id);
            }
            else if (existingHash != newHash)
            {
                delta.Changed.Add(question.Id);
            }
        }

        foreach (int existingId in existingHashes.Keys)
        {
            if (!jsonIds.Contains(existingId))
            {
                delta.Removed.Add(existingId);
            }
        }

        return delta;
    }

    private static void ApplyDelta(string dbPath, List<Question> questions, SyncDelta delta)
    {
        if (delta.IsEmpty)
        {
            return;
        }

        Dictionary<int, Question> questionMap = [];
        foreach (Question q in questions)
        {
            questionMap[q.Id] = q;
        }

        using SqliteConnection connection = new($"Data Source={dbPath}");
        connection.Open();

        using SqliteTransaction transaction = connection.BeginTransaction();

        foreach (int id in delta.Added)
        {
            InsertQuestion(connection, transaction, questionMap[id]);
        }

        foreach (int id in delta.Changed)
        {
            UpdateQuestion(connection, transaction, questionMap[id]);
        }

        foreach (int id in delta.Removed)
        {
            connection.Execute("DELETE FROM questions WHERE id = @Id;",
                new { Id = id }, transaction);
        }

        transaction.Commit();
    }

    private static void InsertQuestion(SqliteConnection connection, SqliteTransaction transaction, Question q)
    {
        connection.Execute(@"
            INSERT INTO questions
                (id, olympiad, stage, year, difficulty, source, source_url, source_raw,
                 explanation_source, type, content, content_cpp, options, match_options,
                 correct_answer, category, algorithms, explanation, points, partial_credit, content_hash)
            VALUES
                (@Id, @Olympiad, @Stage, @Year, @Difficulty, @Source, @SourceUrl, @SourceRaw,
                 @ExplanationSource, @Type, @Content, @ContentCpp, @Options, @MatchOptions,
                 @CorrectAnswer, @Category, @Algorithms, @Explanation, @Points, @PartialCredit, @ContentHash);",
            ToParameters(q),
            transaction);
    }

    private static void UpdateQuestion(SqliteConnection connection, SqliteTransaction transaction, Question q)
    {
        connection.Execute(@"
            UPDATE questions SET
                olympiad = @Olympiad, stage = @Stage, year = @Year, difficulty = @Difficulty,
                source = @Source, source_url = @SourceUrl, source_raw = @SourceRaw,
                explanation_source = @ExplanationSource, type = @Type, content = @Content,
                content_cpp = @ContentCpp, options = @Options, match_options = @MatchOptions,
                correct_answer = @CorrectAnswer, category = @Category, algorithms = @Algorithms,
                explanation = @Explanation, points = @Points, partial_credit = @PartialCredit,
                content_hash = @ContentHash
            WHERE id = @Id;",
            ToParameters(q),
            transaction);
    }

    private static object ToParameters(Question q)
    {
        return new
        {
            Id                = q.Id,
            Olympiad          = q.Olympiad,
            Stage             = q.Stage,
            Year              = (object)q.Year ?? DBNull.Value,
            Difficulty        = (object)q.Difficulty ?? DBNull.Value,
            Source            = (object)q.Source ?? DBNull.Value,
            SourceUrl         = (object)q.SourceUrl ?? DBNull.Value,
            SourceRaw         = (object)q.SourceRaw ?? DBNull.Value,
            ExplanationSource = (object)q.ExplanationSource ?? DBNull.Value,
            Type              = FormatType(q.Type),
            Content           = JsonList(q.Content),
            ContentCpp        = q.ContentCpp != null ? JsonList(q.ContentCpp) : (object)DBNull.Value,
            Options           = JsonList(q.Options),
            MatchOptions      = q.MatchOptions != null ? JsonList(q.MatchOptions) : (object)DBNull.Value,
            CorrectAnswer     = JsonList(q.CorrectAnswer),
            Category          = JsonList(q.Category),
            Algorithms        = JsonList(q.Algorithms),
            Explanation       = q.Explanation != null ? JsonList(q.Explanation) : (object)DBNull.Value,
            Points            = q.Points,
            PartialCredit     = q.PartialCredit ? 1 : 0,
            ContentHash       = ComputeHash(q)
        };
    }

    private static string JsonList<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return "[]";
        }
        return JsonSerializer.Serialize(list, _hashOptions);
    }

    private static string FormatType(QuestionType type)
    {
        return type switch
        {
            QuestionType.Single      => "single",
            QuestionType.Multi       => "multi",
            QuestionType.ShortAnswer => "shortAnswer",
            QuestionType.TrueFalse   => "trueFalse",
            QuestionType.Ordering    => "ordering",
            QuestionType.Matching    => "matching",
            _                        => "unknown"
        };
    }

    private static string ComputeHash(Question question)
    {
        string json = JsonSerializer.Serialize(question, _hashOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
