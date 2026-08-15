using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderScoringTests
{
    [Fact]
    public void Compute_ReturnsMaxPoints_WhenAllMatched()
    {
        GradeResult result = GraderScoring.Compute(1, 1, 5.0, false, false);

        Assert.True(result.IsCorrect);
        Assert.Equal(5.0, result.PointsAwarded);
        Assert.Equal(5.0, result.MaxPoints);
    }

    [Fact]
    public void Compute_ReturnsIsCorrectFalse_WhenMaxPointsIsZero()
    {
        GradeResult result = GraderScoring.Compute(1, 1, 0.0, false, false);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.MaxPoints);
    }

    [Fact]
    public void Compute_ReturnsIsCorrectTrue_WhenAllPositionsMatchedWithPartialCredit()
    {
        GradeResult result = GraderScoring.Compute(3, 3, 3.0, true, true);

        Assert.True(result.IsCorrect);
        Assert.Equal(3.0, result.PointsAwarded);
    }

    [Fact]
    public void Compute_ReturnsProportionalPoints_WhenOneOfThreePositionsMatchedWithPartialCredit()
    {
        GradeResult result = GraderScoring.Compute(1, 3, 3.0, true, true);

        Assert.True(Math.Abs(result.PointsAwarded - 1.0) < 1e-9);
        Assert.False(result.IsCorrect);
    }
}
