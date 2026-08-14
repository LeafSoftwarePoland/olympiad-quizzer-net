namespace OlympiadQuizzer.Core.Domain.Grading;

public sealed record GradeResult(bool IsCorrect, double PointsAwarded, double MaxPoints);
