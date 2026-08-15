using Microsoft.Extensions.Logging;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

internal sealed class CapturingLogger<T> : ILogger<T>
{
    private readonly List<CapturedLogEntry> _entries = [];

    internal IReadOnlyList<CapturedLogEntry> Entries => _entries;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return new NoScope();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        _entries.Add(new CapturedLogEntry(logLevel, formatter(state, exception)));
    }

    private sealed class NoScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
