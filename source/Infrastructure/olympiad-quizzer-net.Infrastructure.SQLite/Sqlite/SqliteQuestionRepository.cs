using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

public sealed class SqliteQuestionRepository : IQuestionRepository
{
    private static readonly StringComparer _tagComparer = StringComparer.OrdinalIgnoreCase;

    private readonly IQuestionStore _store;
    private readonly IShuffler _shuffler;
    private readonly ILogger<SqliteQuestionRepository> _logger;

    private readonly FilterOptions _cachedFilterOptions;
    private readonly HashSet<string> _knownCategories;
    private readonly HashSet<string> _knownAlgorithms;
    private readonly HashSet<string> _knownStages;
    private readonly HashSet<int> _knownYears;

    public SqliteQuestionRepository(
        IQuestionStore store,
        IShuffler shuffler,
        ILogger<SqliteQuestionRepository> logger)
    {
        _store = store;
        _shuffler = shuffler;
        _logger = logger;

        BankSummary summary = _store.LoadSummary();

        IEnumerable<string> allCategories = summary.CategoryJsons.SelectMany(ParseJsonArray);
        IEnumerable<string> allAlgorithms = summary.AlgorithmJsons.SelectMany(ParseJsonArray);

        _knownCategories = DistinctValues(allCategories);
        _knownAlgorithms = DistinctValues(allAlgorithms);
        _knownStages     = DistinctValues(summary.Stages.Select(s => s.Value));
        _knownYears      = new HashSet<int>(
            summary.Years.Select(y => int.Parse(y.Value, CultureInfo.InvariantCulture)));

        _cachedFilterOptions = new FilterOptions
        {
            TotalQuestions = summary.TotalCount,
            Stages     = [.. summary.Stages],
            Years      = [.. summary.Years],
            Categories = CountBy(allCategories),
            Algorithms = CountBy(allAlgorithms)
        };

        _logger.LogInformation(
            "SqliteQuestionRepository initialised: {TotalCount} questions.",
            summary.TotalCount);
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

        IReadOnlyList<QuestionCandidate> candidates = _store.SelectCandidates(stages, years);

        IEnumerable<QuestionCandidate> matched = candidates;

        if (categories.Count > 0)
        {
            matched = matched.Where(c => HasAny(ParseJsonArray(c.Category), categories));
        }

        if (algorithms.Count > 0)
        {
            matched = matched.Where(c => HasAny(ParseJsonArray(c.Algorithms), algorithms));
        }

        List<QuestionCandidate> matchedList = [.. matched];
        int matchedCount = matchedList.Count;

        // Shuffle before capping: capping first would make the result deterministic by bank order.
        _shuffler.Shuffle(matchedList);

        IReadOnlyList<int> selectedIds = matchedList.Count > limit
            ? [.. matchedList.Take(limit).Select(c => c.Id)]
            : [.. matchedList.Select(c => c.Id)];

        IReadOnlyList<QuestionRow> rows = _store.FetchByIds(selectedIds);

        Dictionary<int, QuestionRow> rowById = [];
        foreach (QuestionRow row in rows)
        {
            rowById[row.Id] = row;
        }

        List<Question> result = [.. selectedIds.Where(rowById.ContainsKey).Select(id => FromRow(rowById[id]))];

        _logger.LogInformation(
            "Question query served: matched={MatchedCount} returned={ReturnedCount} limit={Limit} " +
            "categories={Categories} algorithms={Algorithms} years={Years} stages={Stages}",
            matchedCount, result.Count, limit,
            string.Join(",", categories), string.Join(",", algorithms),
            string.Join(",", years), string.Join(",", stages));

        if (result.Count == 0)
        {
            _logger.LogWarning("Question query matched nothing.");
        }

        return Task.FromResult<IReadOnlyList<Question>>(result);
    }

    public Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_cachedFilterOptions);
    }

    private static Question FromRow(QuestionRow row)
    {
        return new Question
        {
            Id                = row.Id,
            Olympiad          = row.Olympiad,
            Stage             = row.Stage,
            Year              = row.Year,
            Difficulty        = row.Difficulty,
            Source            = row.Source,
            SourceUrl         = row.SourceUrl,
            SourceRaw         = row.SourceRaw,
            ExplanationSource = row.ExplanationSource,
            Type              = ParseType(row.Type),
            Content           = DeserializeList<ContentBlock>(row.Content) ?? [],
            ContentCpp        = string.IsNullOrEmpty(row.ContentCpp) ? null : DeserializeList<ContentBlock>(row.ContentCpp),
            Options           = DeserializeList<string>(row.Options) ?? [],
            MatchOptions      = string.IsNullOrEmpty(row.MatchOptions) ? null : DeserializeList<string>(row.MatchOptions),
            CorrectAnswer     = DeserializeList<string>(row.CorrectAnswer) ?? [],
            Category          = DeserializeList<string>(row.Category) ?? [],
            Algorithms        = DeserializeList<string>(row.Algorithms) ?? [],
            Explanation       = string.IsNullOrEmpty(row.Explanation) ? null : DeserializeList<ContentBlock>(row.Explanation),
            Points            = row.Points,
            PartialCredit     = row.PartialCredit != 0
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

    private static IEnumerable<string> ParseJsonArray(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }
        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions.Default) ?? [];
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
            _             => throw new InvalidOperationException($"Unrecognised question type: '{typeString}'.")
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

    private static bool HasAny(IEnumerable<string> questionValues, HashSet<string> wanted)
    {
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
}
