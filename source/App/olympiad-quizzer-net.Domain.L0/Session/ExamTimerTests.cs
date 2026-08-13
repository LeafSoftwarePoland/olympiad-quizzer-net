using OlympiadQuizzer.Domain.Session;

namespace OlympiadQuizzer.Domain.L0.Session;

[Trait("Tier", "L0")]
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
        QuizSessionState state = new QuizSessionState
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
