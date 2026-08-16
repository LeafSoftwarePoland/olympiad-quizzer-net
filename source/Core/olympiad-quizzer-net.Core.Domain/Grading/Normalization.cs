using System.Text;

namespace OlympiadQuizzer.Core.Domain.Grading;

public static class Normalization
{
    public static string NormalizeChoice(string value)
    {
        if (value == null || value.Length == 0)
        {
            return string.Empty;
        }

        return value.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    public static string NormalizeFreeText(string value)
    {
        if (value == null || value.Length == 0)
        {
            return string.Empty;
        }

        return CollapseWhitespaceRuns(value.Trim().Normalize(NormalizationForm.FormKC).ToLowerInvariant());
    }

    internal static string CollapseWhitespaceRuns(string value)
    {
        var builder = new StringBuilder(value.Length);
        bool previousWasWhitespace = false;

        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];
            if (char.IsWhiteSpace(current))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                }
                previousWasWhitespace = true;
                continue;
            }

            builder.Append(current);
            previousWasWhitespace = false;
        }

        return builder.ToString();
    }
}
