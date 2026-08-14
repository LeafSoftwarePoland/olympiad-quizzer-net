using System.Text;
using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public static class Grader
{
    /// Closed-list comparison: the submitted value came from `options`, so only
    /// case, surrounding whitespace and composed/decomposed accents may differ.
    public static string NormalizeChoice(string value)
    {
        if (value == null || value.Length == 0)
        {
            return string.Empty;
        }

        return value.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    /// Free-text comparison: the student typed it. FormKC additionally folds the
    /// compatibility characters the source PDFs are full of — subscripts, superscripts,
    /// mathematical italics, non-breaking spaces.
    public static string NormalizeFreeText(string value)
    {
        if (value == null || value.Length == 0)
        {
            return string.Empty;
        }

        return CollapseWhitespaceRuns(value.Trim().Normalize(NormalizationForm.FormKC).ToLowerInvariant());
    }

    public static GradeResult Grade(Question question, SubmittedAnswer answer)
    {
        if (question == null)
        {
            return new GradeResult(false, 0, 0);
        }

        double max = question.Points;

        if (answer == null || answer.IsEmpty)
        {
            return new GradeResult(false, 0, max);
        }

        var expected = question.CorrectAnswer;
        expected ??= [];

        int matched;
        int total;

        switch (question.Type)
        {
            case QuestionType.Single:
                total = 1;
                if (expected.Count == 1 && answer.Values.Count == 1 &&
                    NormalizeChoice(answer.Values[0]) == NormalizeChoice(expected[0]))
                {
                    matched = 1;
                }
                else
                {
                    matched = 0;
                }
                break;

            case QuestionType.ShortAnswer:
                total = 1;
                if (expected.Count == 1 && answer.Values.Count == 1 &&
                    NormalizeFreeText(answer.Values[0]) == NormalizeFreeText(expected[0]))
                {
                    matched = 1;
                }
                else
                {
                    matched = 0;
                }
                break;

            case QuestionType.Multi:
                total = 1;
                matched = SetEqual(answer.Values, expected) ? 1 : 0;
                break;

            case QuestionType.TrueFalse:
            case QuestionType.Ordering:
            case QuestionType.Matching:
                (matched, total) = PositionMatch(answer.Values, expected);
                break;

            default:
                return new GradeResult(false, 0, max);
        }

        if (total <= 0)
        {
            return new GradeResult(false, 0, max);
        }

        bool positional = question.Type == QuestionType.TrueFalse ||
                          question.Type == QuestionType.Ordering ||
                          question.Type == QuestionType.Matching;

        double awarded;
        if (positional && question.PartialCredit)
        {
            awarded = max * matched / total;
        }
        else
        {
            awarded = matched == total ? max : 0;
        }

        bool isCorrect = max > 0 && Math.Abs(awarded - max) < 1e-9;

        return new GradeResult(isCorrect, awarded, max);
    }

    private static bool SetEqual(List<string> submitted, List<string> expected)
    {
        var submittedSet = new HashSet<string>(
            submitted.Select(NormalizeChoice), StringComparer.Ordinal);
        var expectedSet = new HashSet<string>(
            expected.Select(NormalizeChoice), StringComparer.Ordinal);
        return submittedSet.SetEquals(expectedSet);
    }

    private static (int matched, int total) PositionMatch(List<string> submitted, List<string> expected)
    {
        int total = expected.Count;
        if (submitted.Count != total)
        {
            return (0, total);
        }

        int matched = 0;
        for (int i = 0; i < total; i++)
        {
            if (NormalizeChoice(submitted[i]) == NormalizeChoice(expected[i]))
            {
                matched++;
            }
        }
        return (matched, total);
    }

    private static string CollapseWhitespaceRuns(string value)
    {
        var builder = new StringBuilder(value.Length);
        bool previousWasWhitespace = false;

        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];
            if (char.IsWhiteSpace(current))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                }
                previousWasWhitespace = true;
                continue;
            }

            builder.Append(current);
            previousWasWhitespace = false;
        }

        return builder.ToString();
    }
}
