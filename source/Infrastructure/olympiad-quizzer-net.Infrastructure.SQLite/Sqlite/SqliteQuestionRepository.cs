using System.Globalization;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

public sealed class SqliteQuestionRepository : IQuestionRepository
{
    private const int SchemaVersion = 1;
    private static readonly StringComparer _tagComparer = StringComparer.OrdinalIgnoreCase;

    private readonly string _databasePath;
    private readonly IShuffler _shuffler;
    private readonly ILogger<SqliteQuestionRepository> _logger;

    private readonly List<Question> _allQuestions;
    private readonly HashSet<string> _knownCategories;
    private readonly HashSet<string> _knownAlgorithms;
    private readonly HashSet<string> _knownStages;
    private readonly HashSet<int> _knownYears;

    public SqliteQuestionRepository(
        IOptions<QuestionBankOptions> options,
        IShuffler shuffler,
        ILogger<SqliteQuestionRepository> logger)
    {
        string configured = options.Value.DatabasePath;
        string path = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Question database not found: {path}");
        }

        _databasePath = path;
        _shuffler = shuffler;
        _logger = logger;

        using (SqliteConnection connection = new($"Data Source={_databasePath};Mode=ReadOnly"))
        {
            connection.Open();
            long version = connection.ExecuteScalar<long>("PRAGMA user_version;");
            if (version != SchemaVersion)
            {
                throw new InvalidOperationException(
                    $"Database schema version mismatch: expected {SchemaVersion}, found {version}.");
            }
        }

        _allQuestions = LoadAll();

        _knownCategories = DistinctValues(_allQuestions.SelectMany(q => q.Category ?? []));
        _knownAlgorithms = DistinctValues(_allQuestions.SelectMany(q => q.Algorithms ?? []));
        _knownStages     = DistinctValues(_allQuestions.Select(q => q.Stage));
        _knownYears      = new HashSet<int>(_allQuestions.Where(q => q.Year.HasValue).Select(q => q.Year.Value));

        _logger.LogInformation("SqliteQuestionRepository loaded {Count} questions from {Path}",
            _allQuestions.Count, _databasePath);
    }

    public Task<IReadOnlyList<Question>> GetAsync(QuestionQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int limit = ClampLimit(query.Limit);

        HashSet<string> categories = ToSet(query.Categories);
        HashSet<string> algorithms = ToSet(query.Algorithms);
        HashSet<string> stages     = ToSet(query.Stages);
        HashSet<int>    years      = ToYearSet(query.Years);

        WarnOnUnknownValues(categories, algorithms, stages, years);

        IEnumerable<Question> candidates = _allQuestions;

        if (categories.Count > 0)
        {
            candidates = candidates.Where(q => HasAny(q.Category, categories));
        }

        if (algorithms.Count > 0)
        {
            candidates = candidates.Where(q => HasAny(q.Algorithms, algorithms));
        }

        if (years.Count > 0)
        {
            candidates = candidates.Where(q => q.Year.HasValue && years.Contains(q.Year.Value));
        }

        if (stages.Count > 0)
        {
            candidates = candidates.Where(q => q.Stage != null && stages.Contains(q.Stage));
        }

        List<Question> matched = [.. candidates];
        int matchedCount = matched.Count;

        _shuffler.Shuffle(matched);

        List<Question> selected = matched.Count > limit
            ? matched.GetRange(0, limit)
            : matched;

        _logger.LogInformation(
            "Question query served: matched={MatchedCount} returned={ReturnedCount} limit={Limit} " +
            "categories={Categories} algorithms={Algorithms} years={Years} stages={Stages}",
            matchedCount, selected.Count, limit,
            string.Join(",", categories), string.Join(",", algorithms),
            string.Join(",", years), string.Join(",", stages));

        if (selected.Count == 0)
        {
            _logger.LogWarning("Question query matched nothing.");
        }

        return Task.FromResult<IReadOnlyList<Question>>(selected);
    }

    public Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        FilterOptions options = new()
        {
            TotalQuestions = _allQuestions.Count,
            Categories = CountBy(_allQuestions.SelectMany(q => q.Category ?? [])),
            Algorithms = CountBy(_allQuestions.SelectMany(q => q.Algorithms ?? [])),
            Stages     = CountBy(_allQuestions.Select(q => q.Stage)),
            Years      = CountBy(_allQuestions.Where(q => q.Year.HasValue)
                                              .Select(q => q.Year.Value.ToString(CultureInfo.InvariantCulture)))
        };

        return Task.FromResult(options);
    }

    private List<Question> LoadAll()
    {
        using SqliteConnection connection = new($"Data Source={_databasePath};Mode=ReadOnly");
        connection.Open();

        IEnumerable<QuestionRow> rows = connection.Query<QuestionRow>("SELECT * FROM questions;");
        List<Question> questions = [];
        foreach (QuestionRow row in rows)
        {
            questions.Add(FromRow(row));
        }
        return questions;
    }

    private static Question FromRow(QuestionRow row)
    {
        return new Question
        {
            Id                = row.id,
            Olympiad          = row.olympiad,
            Stage             = row.stage,
            Year              = row.year,
            Difficulty        = row.difficulty,
            Source            = row.source,
            SourceUrl         = row.source_url,
            SourceRaw         = row.source_raw,
            ExplanationSource = row.explanation_source,
            Type              = ParseType(row.type),
            Content           = DeserializeList<ContentBlock>(row.content) ?? [],
            ContentCpp        = string.IsNullOrEmpty(row.content_cpp) ? null : DeserializeList<ContentBlock>(row.content_cpp),
            Options           = DeserializeList<string>(row.options) ?? [],
            MatchOptions      = string.IsNullOrEmpty(row.match_options) ? null : DeserializeList<string>(row.match_options),
            CorrectAnswer     = DeserializeList<string>(row.correct_answer) ?? [],
            Category          = DeserializeList<string>(row.category) ?? [],
            Algorithms        = DeserializeList<string>(row.algorithms) ?? [],
            Explanation       = string.IsNullOrEmpty(row.explanation) ? null : DeserializeList<ContentBlock>(row.explanation),
            Points            = row.points,
            PartialCredit     = row.partial_credit != 0
        };
    }

    private static List<T> DeserializeList<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions.Default);
    }

    private static QuestionType ParseType(string typeString)
    {
        return typeString switch
        {
            "single"      => QuestionType.Single,
            "multi"       => QuestionType.Multi,
            "shortAnswer" => QuestionType.ShortAnswer,
            "trueFalse"   => QuestionType.TrueFalse,
            "ordering"    => QuestionType.Ordering,
            "matching"    => QuestionType.Matching,
            _             => QuestionType.Unknown
        };
    }

    private static int ClampLimit(int requested)
    {
        if (requested <= 0)
        {
            return QuestionQuery.DefaultLimit;
        }
        return requested > QuestionQuery.MaxLimit ? QuestionQuery.MaxLimit : requested;
    }

    private static HashSet<string> ToSet(List<string> values)
    {
        HashSet<string> set = new(_tagComparer);
        if (values == null)
        {
            return set;
        }
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                set.Add(value.Trim());
            }
        }
        return set;
    }

    private static HashSet<int> ToYearSet(List<int> values)
    {
        if (values == null)
        {
            return [];
        }
        return new HashSet<int>(values);
    }

    private static HashSet<string> DistinctValues(IEnumerable<string> values)
    {
        HashSet<string> set = new(_tagComparer);
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                set.Add(value);
            }
        }
        return set;
    }

    private static bool HasAny(List<string> questionValues, HashSet<string> wanted)
    {
        if (questionValues == null)
        {
            return false;
        }
        foreach (string value in questionValues)
        {
            if (wanted.Contains(value))
            {
                return true;
            }
        }
        return false;
    }

    private static List<FilterOption> CountBy(IEnumerable<string> values)
    {
        return [.. values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, _tagComparer)
            .Select(group => new FilterOption { Value = group.Key, Count = group.Count() })
            .OrderBy(option => option.Value, _tagComparer)];
    }

    private void WarnOnUnknownValues(
        HashSet<string> categories,
        HashSet<string> algorithms,
        HashSet<string> stages,
        HashSet<int> years)
    {
        List<string> unknown = [];
        Collect(unknown, "category",   categories, _knownCategories);
        Collect(unknown, "algorithms", algorithms, _knownAlgorithms);
        Collect(unknown, "stage",      stages,     _knownStages);

        foreach (int year in years)
        {
            if (!_knownYears.Contains(year))
            {
                unknown.Add("year=" + year.ToString(CultureInfo.InvariantCulture));
            }
        }

        if (unknown.Count == 0)
        {
            return;
        }

        _logger.LogWarning(
            "Question query carried values not present in the bank: {UnknownValues}",
            string.Join(", ", unknown));
    }

    private static void Collect(
        List<string> unknown, string parameterName, HashSet<string> requested, HashSet<string> known)
    {
        foreach (string value in requested)
        {
            if (!known.Contains(value))
            {
                unknown.Add(parameterName + "=" + value);
            }
        }
    }

    private sealed class QuestionRow
    {
        public int    id                 { get; set; }
        public string olympiad           { get; set; }
        public string stage              { get; set; }
        public int?   year               { get; set; }
        public int?   difficulty         { get; set; }
        public string source             { get; set; }
        public string source_url         { get; set; }
        public string source_raw         { get; set; }
        public string explanation_source { get; set; }
        public string type               { get; set; }
        public string content            { get; set; }
        public string content_cpp        { get; set; }
        public string options            { get; set; }
        public string match_options      { get; set; }
        public string correct_answer     { get; set; }
        public string category           { get; set; }
        public string algorithms         { get; set; }
        public string explanation        { get; set; }
        public int    points             { get; set; }
        public int    partial_credit     { get; set; }
        public string content_hash       { get; set; }
    }
}
