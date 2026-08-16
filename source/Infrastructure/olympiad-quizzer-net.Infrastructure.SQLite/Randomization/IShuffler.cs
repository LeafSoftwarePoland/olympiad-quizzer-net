namespace OlympiadQuizzer.Infrastructure.SQLite.Randomization;

public interface IShuffler
{
    void Shuffle<T>(IList<T> items);
}
