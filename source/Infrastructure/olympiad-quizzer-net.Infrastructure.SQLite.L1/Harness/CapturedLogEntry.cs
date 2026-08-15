using Microsoft.Extensions.Logging;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

internal sealed class CapturedLogEntry
{
    public CapturedLogEntry(LogLevel level, string message)
    {
        Level = level;
        Message = message;
    }

    public LogLevel Level { get; }

    public string Message { get; }
}
