namespace OlympiadQuizzer.Infrastructure.SQLite.Randomization;

public sealed class FisherYatesShuffler : IShuffler
{
    public void Shuffle<T>(IList<T> items)
    {
        // Random.Shared is thread-safe. A shared instance created with new Random() is not.
        Permutation.FisherYates(items, Random.Shared);
    }
}
