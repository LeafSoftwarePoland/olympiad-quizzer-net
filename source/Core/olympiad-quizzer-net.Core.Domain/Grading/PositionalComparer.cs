namespace OlympiadQuizzer.Core.Domain.Grading;

public static class PositionalComparer
{
    public static (int matched, int total) PositionMatch(List<string> submitted, List<string> expected)
    {
        int total = expected.Count;
        if (submitted.Count != total)
        {
            return (0, total);
        }

        int matched = 0;
        for (int i = 0; i < total; i++)
        {
            if (Normalization.NormalizeChoice(submitted[i]) == Normalization.NormalizeChoice(expected[i]))
            {
                matched++;
            }
        }
        return (matched, total);
    }
}
