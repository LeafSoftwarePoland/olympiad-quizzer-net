using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class PositionalComparerTests
{
    [Fact]
    public void PositionMatch_ReturnsAllMatchedAndCorrectTotal_WhenAllPositionsMatch()
    {
        // Arrange
        List<string> submitted = ["a", "b", "c"];
        List<string> expected  = ["a", "b", "c"];

        // Act
        (int matched, int total) = PositionalComparer.PositionMatch(submitted, expected);

        // Assert
        Assert.Equal(3, matched);
        Assert.Equal(3, total);
    }

    [Fact]
    public void PositionMatch_ReturnsZeroMatchedAndCorrectTotal_WhenNoPositionsMatch()
    {
        // Arrange
        List<string> submitted = ["x", "y", "z"];
        List<string> expected  = ["a", "b", "c"];

        // Act
        (int matched, int total) = PositionalComparer.PositionMatch(submitted, expected);

        // Assert
        Assert.Equal(0, matched);
        Assert.Equal(3, total);
    }

    [Fact]
    public void PositionMatch_ReturnsZeroMatchedAndCorrectTotal_WhenLengthsDiffer()
    {
        // Arrange
        List<string> submitted = ["a", "b"];
        List<string> expected  = ["a", "b", "c"];

        // Act
        (int matched, int total) = PositionalComparer.PositionMatch(submitted, expected);

        // Assert
        Assert.Equal(0, matched);
        Assert.Equal(3, total);
    }

    [Fact]
    public void PositionMatch_ReturnsOneMatchedAndCorrectTotal_WhenOneOfThreePositionsMatches()
    {
        // Arrange
        List<string> submitted = ["a", "x", "y"];
        List<string> expected  = ["a", "b", "c"];

        // Act
        (int matched, int total) = PositionalComparer.PositionMatch(submitted, expected);

        // Assert
        Assert.Equal(1, matched);
        Assert.Equal(3, total);
    }

    [Fact]
    public void PositionMatch_ReturnsMatchedIgnoringCase_WhenValuesDifferInCasingOnly()
    {
        // Arrange
        List<string> submitted = ["True", "FALSE"];
        List<string> expected  = ["true", "false"];

        // Act
        (int matched, int total) = PositionalComparer.PositionMatch(submitted, expected);

        // Assert
        Assert.Equal(2, matched);
        Assert.Equal(2, total);
    }
}
