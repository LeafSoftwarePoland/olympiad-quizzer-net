namespace OlympiadQuizzer.Infrastructure.SQLite.Sync;

public sealed class SyncDelta
{
    public List<int> Added   { get; set; } = [];
    public List<int> Changed { get; set; } = [];
    public List<int> Removed { get; set; } = [];

    public bool IsEmpty => Added.Count == 0 && Changed.Count == 0 && Removed.Count == 0;

    public string FormatReport()
    {
        if (IsEmpty)
        {
            return "No changes detected.";
        }

        List<string> lines = [];
        if (Added.Count > 0)
        {
            lines.Add($"Added ({Added.Count}): {string.Join(", ", Added)}");
        }
        if (Changed.Count > 0)
        {
            lines.Add($"Changed ({Changed.Count}): {string.Join(", ", Changed)}");
        }
        if (Removed.Count > 0)
        {
            lines.Add($"Removed ({Removed.Count}): {string.Join(", ", Removed)}");
        }
        return string.Join(Environment.NewLine, lines);
    }
}
