namespace OlympiadQuizzer.Core.Domain.Session;

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

    // Total minutes, never the TimeSpan minutes component. A standard "mm" format counts 0-59, so
    // it silently discards the hour: ninety minutes is 01:30:00 and renders as "30:00", which reads
    // as a limit a third of its real size. Total minutes also matches how an exam is counted down.
    public static string Format(TimeSpan remaining)
    {
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        int totalMinutes = (int)remaining.TotalMinutes;
        return $"{totalMinutes:D2}:{remaining.Seconds:D2}";
    }
}
