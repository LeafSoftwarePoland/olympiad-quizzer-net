using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Session;

namespace OlympiadQuizzer.Core.Domain.L0.Session;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class ExamTimerTests
{
    [Fact]
    public void Remaining_WithHalfTheLimitElapsed_ReturnsHalfTheLimit()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-30);
        QuizSessionState state = TimedState(start, timeLimitMinutes: 60);
        DateTimeOffset now = start.AddMinutes(30);

        TimeSpan remaining = ExamTimer.Remaining(state, now);

        Assert.Equal(TimeSpan.FromMinutes(30), remaining);
    }

    [Fact]
    public void Remaining_WithMoreThanTheLimitElapsed_ReturnsZeroNotNegative()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-90);
        QuizSessionState state = TimedState(start, timeLimitMinutes: 30);
        DateTimeOffset now = start.AddMinutes(90);

        TimeSpan remaining = ExamTimer.Remaining(state, now);

        Assert.Equal(TimeSpan.Zero, remaining);
    }

    [Fact]
    public void Remaining_ForUntimedSession_ReturnsZero()
    {
        QuizSessionState state = new()
		{
            Timed = false,
            StartedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-10)
        };
        DateTimeOffset now = DateTimeOffset.UtcNow;

        TimeSpan remaining = ExamTimer.Remaining(state, now);

        Assert.Equal(TimeSpan.Zero, remaining);
    }

    [Fact]
    public void Remaining_IsUnaffectedByHowManyTimesItIsCalled()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-10);
        QuizSessionState state = TimedState(start, timeLimitMinutes: 60);
        DateTimeOffset now = start.AddMinutes(10);

        TimeSpan first  = ExamTimer.Remaining(state, now);
        TimeSpan second = ExamTimer.Remaining(state, now);
        TimeSpan third  = ExamTimer.Remaining(state, now);

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public void Remaining_WithNullState_ReturnsZero()
    {
        TimeSpan remaining = ExamTimer.Remaining(null, DateTimeOffset.UtcNow);

        Assert.Equal(TimeSpan.Zero, remaining);
    }

    // Regression: the display used a "mm\:ss" format, whose minutes component counts 0-59. The
    // hour was discarded, so a 90-minute limit read as "30:00" and a 120-minute one as "00:00".
    [Theory]
    [InlineData(90, 0, "90:00")]
    [InlineData(89, 57, "89:57")]
    [InlineData(120, 0, "120:00")]
    [InlineData(60, 0, "60:00")]
    [InlineData(59, 59, "59:59")]
    [InlineData(30, 0, "30:00")]
    [InlineData(5, 3, "05:03")]
    [InlineData(0, 45, "00:45")]
    [InlineData(0, 0, "00:00")]
    public void Format_CountsEveryRemainingMinute_WhenRemainingSpansAnHourOrMore(
        int minutes, int seconds, string expected)
    {
        // Arrange
        TimeSpan remaining = new(0, minutes, seconds);

        // Act
        string formatted = ExamTimer.Format(remaining);

        // Assert
        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void Format_ReturnsZero_WhenRemainingIsNegative()
    {
        // Arrange
        const string expired = "00:00";
        TimeSpan overdue = TimeSpan.FromMinutes(-5);

        // Act
        string formatted = ExamTimer.Format(overdue);

        // Assert
        Assert.Equal(expired, formatted);
    }

    private static QuizSessionState TimedState(DateTimeOffset startedAt, int timeLimitMinutes)
    {
        return new QuizSessionState
        {
            Timed = true,
            TimeLimitMinutes = timeLimitMinutes,
            StartedAtUtc = startedAt
        };
    }
}
