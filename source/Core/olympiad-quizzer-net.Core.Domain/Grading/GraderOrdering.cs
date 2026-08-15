using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public sealed class GraderOrdering : IQuestionGrader
{
    public QuestionType QuestionType => QuestionType.Ordering;

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

        (int matched, int total) = PositionalComparer.PositionMatch(answer.Values, expected);

        return GraderScoring.Compute(matched, total, max, true, question.PartialCredit);
    }
}
