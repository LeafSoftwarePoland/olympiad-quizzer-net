using System.Text;

namespace OlympiadQuizzer.Shared;

public static class Grader
{
    public static string Normalize(string? s) =>
        (s ?? string.Empty).Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

    public static GradeResult Grade(Question q, AnswerSubmission? a)
    {
        double max = q.Points;
        if (a is null) return new GradeResult(false, 0, max);

        var (matched, total) = q.Type switch
        {
            QuestionType.MultiSelect => SetEqual(a.SelectedIndices, q.CorrectIndices()) ? (1, 1) : (0, 1),
            QuestionType.SingleAbcd  => SeqEqual(a.SelectedIndices, q.CorrectIndices()) ? (1, 1) : (0, 1),
            QuestionType.ShortAnswer => q.CorrectStrings().Any(e => Normalize(e) == Normalize(a.Text)) ? (1, 1) : (0, 1),
            QuestionType.TrueFalse   => PositionMatch(a.Booleans, q.CorrectBooleans()),
            QuestionType.Ordering    => PositionMatch(a.Order,    q.CorrectIndices()),
            QuestionType.Matching    => PositionMatch(a.Matches,  q.CorrectIndices()),
            _                        => (0, 1)
        };

        if (total <= 0) return new GradeResult(false, 0, max);

        bool positional = q.Type is QuestionType.TrueFalse or QuestionType.Ordering or QuestionType.Matching;
        double awarded  = (positional && q.PartialCredit)
            ? max * matched / total
            : (matched == total ? max : 0);

        return new GradeResult(Math.Abs(awarded - max) < 1e-9 && max > 0, awarded, max);
    }

    private static bool SetEqual(List<int> submitted, int[] expected)
    {
        var a = submitted.Distinct().OrderBy(x => x).ToArray();
        var b = expected.Distinct().OrderBy(x => x).ToArray();
        return a.SequenceEqual(b);
    }

    private static bool SeqEqual(List<int> submitted, int[] expected) =>
        submitted.Count == expected.Length && submitted.SequenceEqual(expected);

    private static (int matched, int total) PositionMatch(List<bool?> submitted, bool[] expected)
    {
        int total = expected.Length;
        if (submitted.Count != total) return (0, total);
        int matched = 0;
        for (int i = 0; i < total; i++)
            if (submitted[i] is bool v && v == expected[i]) matched++;
        return (matched, total);
    }

    private static (int matched, int total) PositionMatch(List<int> submitted, int[] expected)
    {
        int total = expected.Length;
        if (submitted.Count != total) return (0, total);
        int matched = 0;
        for (int i = 0; i < total; i++)
            if (submitted[i] != -1 && submitted[i] == expected[i]) matched++;
        return (matched, total);
    }
}
