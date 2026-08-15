using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public static class GraderPositional
{
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

        List<string> expected = question.CorrectAnswer ?? [];

        (int matched, int total) = PositionMatch(answer.Values, expected);

        return GraderScoring.Compute(matched, total, max, true, question.PartialCredit);
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
            if (Normalization.NormalizeChoice(submitted[i]) == Normalization.NormalizeChoice(expected[i]))
            {
                matched++;
            }
        }
        return (matched, total);
    }
}
