using Microsoft.Extensions.Logging;

namespace OlympiadQuizzer.Core.Tests.Common.Harness;

public sealed class CapturedLogEntry
{
    public CapturedLogEntry(LogLevel level, string message)
    {
        Level = level;
        Message = message;
    }

    public LogLevel Level { get; }

    public string Message { get; }
}
