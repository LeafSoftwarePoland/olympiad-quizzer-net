namespace OlympiadQuizzer.Infrastructure.SQLite.Randomization;

public sealed class SeededShuffler : IShuffler
{
    private readonly int _seed;

    public SeededShuffler(int seed)
    {
        _seed = seed;
    }

    public void Shuffle<T>(IList<T> items)
    {
        // A fresh Random per call makes the permutation a function of the seed and the input order
        // alone. Holding one Random instance would make every result depend on how many times this
        // instance had already been called, which no test can pin and no reader can predict.
        Permutation.FisherYates(items, new Random(_seed));
    }
}
