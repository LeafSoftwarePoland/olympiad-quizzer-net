using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public sealed class GraderSingle : IQuestionGrader
{
    public QuestionType QuestionType => QuestionType.Single;

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

        int matched = expected.Count == 1 && answer.Values.Count == 1 &&
                      Normalization.NormalizeChoice(answer.Values[0]) == Normalization.NormalizeChoice(expected[0])
            ? 1 : 0;

        return GraderScoring.Compute(matched, 1, max, false, false);
    }
}
