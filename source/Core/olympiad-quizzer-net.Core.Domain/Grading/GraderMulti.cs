using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public sealed class GraderMulti : IQuestionGrader
{
    public QuestionType QuestionType => QuestionType.Multi;

    public GradeResult Grade(Question question, SubmittedAnswer answer)
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

        int matched = SetEqual(answer.Values, expected) ? 1 : 0;

        return GraderScoring.Compute(matched, 1, max, false, false);
    }

    private static bool SetEqual(List<string> submitted, List<string> expected)
    {
        HashSet<string> submittedSet = new(
            submitted.Select(Normalization.NormalizeChoice), StringComparer.Ordinal);
        HashSet<string> expectedSet = new(
            expected.Select(Normalization.NormalizeChoice), StringComparer.Ordinal);
        return submittedSet.SetEquals(expectedSet);
    }
}
