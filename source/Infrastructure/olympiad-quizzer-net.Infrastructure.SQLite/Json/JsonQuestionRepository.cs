using System.Globalization;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Infrastructure.SQLite.Json;

public sealed class JsonQuestionRepository : IQuestionRepository
{
    private static readonly StringComparer _tagComparer = StringComparer.OrdinalIgnoreCase;

    private readonly QuestionBank _bank;
    private readonly IShuffler _shuffler;
    private readonly ILogger<JsonQuestionRepository> _logger;

    private readonly HashSet<string> _knownCategories;
    private readonly HashSet<string> _knownAlgorithms;
    private readonly HashSet<string> _knownStages;
    private readonly HashSet<int> _knownYears;

    public JsonQuestionRepository(
        QuestionBankLoader loader,
        IShuffler shuffler,
        ILogger<JsonQuestionRepository> logger)
    {
        _bank = loader.Bank;
        _shuffler = shuffler;
        _logger = logger;

        // The bank cannot change for the life of the process, so its distinct values are computed
        // once here rather than on every request.
        _knownCategories = DistinctValues(_bank.Questions.SelectMany(q => q.Category ?? []));
        _knownAlgorithms = DistinctValues(_bank.Questions.SelectMany(q => q.Algorithms ?? []));
        _knownStages = DistinctValues(_bank.Questions.Select(q => q.Stage));
        _knownYears = new HashSet<int>(_bank.Questions.Where(q => q.Year.HasValue).Select(q => q.Year.Value));
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

        IEnumerable<Question> candidates = _bank.Questions;

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

        // Shuffle before capping: capping first would make the result deterministic by bank order.
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

        IReadOnlyList<Question> all = _bank.Questions;

        FilterOptions options = new()
		{
            TotalQuestions = all.Count,
            Categories = CountBy(all.SelectMany(q => q.Category ?? [])),
            Algorithms = CountBy(all.SelectMany(q => q.Algorithms ?? [])),
            Stages     = CountBy(all.Select(q => q.Stage)),
            Years      = CountBy(all.Where(q => q.Year.HasValue)
                                    .Select(q => q.Year.Value.ToString(CultureInfo.InvariantCulture)))
        };

        return Task.FromResult(options);
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

        Collect(unknown, "category", categories, _knownCategories);
        Collect(unknown, "algorithms", algorithms, _knownAlgorithms);
        Collect(unknown, "stage", stages, _knownStages);

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
