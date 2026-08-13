namespace OlympiadQuizzer.Domain.Grading;

public sealed record GradeResult(bool IsCorrect, double PointsAwarded, double MaxPoints);
