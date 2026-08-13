namespace OlympiadQuizzer.Infrastructure.SQLite.Randomization;

internal static class Permutation
{
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
