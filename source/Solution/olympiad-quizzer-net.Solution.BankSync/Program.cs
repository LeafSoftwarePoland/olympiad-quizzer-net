using OlympiadQuizzer.Infrastructure.SQLite.Sync;

namespace OlympiadQuizzer.Solution.BankSync;

public class Program
{
    public static int Main(string[] args)
    {
        string mode = "sync";
        string jsonPath = null;
        string dbPath = null;

        int argIndex = 0;
        if (argIndex < args.Length && (args[argIndex] == "sync" || args[argIndex] == "check"))
        {
            mode = args[argIndex++];
        }
        if (argIndex < args.Length)
        {
            jsonPath = args[argIndex++];
        }
        if (argIndex < args.Length)
        {
            dbPath = args[argIndex++];
        }

        if (jsonPath == null || dbPath == null)
        {
            string repoRoot = FindRepoRoot();
            jsonPath ??= Path.Combine(repoRoot, "data", "questions.json");
            dbPath ??= Path.Combine(repoRoot, "data", "questions.db");
        }

        try
        {
            SyncDelta delta;
            if (mode == "check")
            {
                delta = DatabaseSync.Check(jsonPath, dbPath);
                Console.WriteLine(delta.FormatReport());
                return delta.IsEmpty ? 0 : 1;
            }
            else
            {
                delta = DatabaseSync.Sync(jsonPath, dbPath);
                Console.WriteLine(delta.FormatReport());
                return 0;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return 1;
        }
    }

    private static string FindRepoRoot()
    {
        string dir = AppContext.BaseDirectory;
        for (int depth = 0; depth < 10; depth++)
        {
            if (File.Exists(Path.Combine(dir, "OlympiadQuizzer.slnx")))
            {
                return dir;
            }
            if (File.Exists(Path.Combine(dir, "data", "schema.sql")))
            {
                return dir;
            }
            string parent = Path.GetDirectoryName(dir);
            if (parent == null)
            {
                break;
            }
            dir = parent;
        }
        throw new DirectoryNotFoundException(
            "Could not locate repository root (no OlympiadQuizzer.slnx or data/schema.sql found above AppContext.BaseDirectory).");
    }
}
