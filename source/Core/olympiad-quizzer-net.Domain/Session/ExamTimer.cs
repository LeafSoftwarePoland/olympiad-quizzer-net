namespace OlympiadQuizzer.Domain.Session;

public static class ExamTimer
{
    public static TimeSpan Remaining(QuizSessionState state, DateTimeOffset now)
    {
        if (state == null)
        {
            return TimeSpan.Zero;
        }

        if (!state.Timed)
        {
            return TimeSpan.Zero;
        }

        TimeSpan elapsed = now - state.StartedAtUtc;
        TimeSpan limit = TimeSpan.FromMinutes(state.TimeLimitMinutes);
        TimeSpan remaining = limit - elapsed;

        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }
}
