namespace OlympiadQuizzer.Core.Domain.Grading;

public static class GraderScoring
{
    public static GradeResult Compute(int matched, int total, double maxPoints, bool positional, bool partialCredit)
    {
        if (total <= 0)
        {
            return new GradeResult(false, 0, maxPoints);
        }

        double awarded;
        if (positional && partialCredit)
        {
            awarded = maxPoints * matched / total;
        }
        else
        {
            awarded = matched == total ? maxPoints : 0;
        }

        bool isCorrect = maxPoints > 0 && Math.Abs(awarded - maxPoints) < 1e-9;

        return new GradeResult(isCorrect, awarded, maxPoints);
    }
}
