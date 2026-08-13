namespace OlympiadQuizzer.Api.L1.Harness;

internal static class FixturePath
{
    internal static string Resolve(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
    }

    internal static string RepoRoot()
    {
        string dir = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(dir, "OlympiadQuizzer.slnx")))
        {
            dir = Path.GetDirectoryName(dir);
            if (dir == null)
            {
                throw new InvalidOperationException("Could not locate repo root from AppContext.BaseDirectory.");
            }
        }
        return dir;
    }
}
