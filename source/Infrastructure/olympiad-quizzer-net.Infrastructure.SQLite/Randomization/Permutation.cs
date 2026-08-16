namespace OlympiadQuizzer.Infrastructure.SQLite.Randomization;

internal static class Permutation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", 
        "IDE0180:Use tuple to swap values", 
        Justification = "Thoug maybe it gives less code, looks more confusing to read. Prefer simple readability over fancy shorthand syntax sugar.")]
	internal static void FisherYates<T>(IList<T> items, Random random)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            T swap = items[i];
            items[i] = items[j];
            items[j] = swap;
        }
    }
}
