using System.Text;
using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Tests;

public class NormalizationTests
{
    [Fact]
    public void Normalize_TrimsLeadingAndTrailingWhitespace()
    {
        Assert.Equal("abc", Grader.Normalize("  abc  "));
    }

    [Fact]
    public void Normalize_AppliesFormC()
    {
        var precomposed = "kód";
        var decomposed  = precomposed.Normalize(NormalizationForm.FormD);
        Assert.NotEqual(precomposed, decomposed);
        Assert.Equal(Grader.Normalize(precomposed), Grader.Normalize(decomposed));
    }

    [Fact]
    public void Normalize_LowercasesInput()
    {
        Assert.Equal("hello", Grader.Normalize("HELLO"));
        Assert.Equal("hello", Grader.Normalize("Hello"));
    }

    [Fact]
    public void Normalize_IsIdempotent()
    {
        var input = "  Kajak  ";
        Assert.Equal(Grader.Normalize(Grader.Normalize(input)), Grader.Normalize(input));
    }

    [Fact]
    public void Normalize_HandlesNull()
    {
        Assert.Equal(string.Empty, Grader.Normalize(null));
    }

    [Fact]
    public void Normalize_HandlesEmpty()
    {
        Assert.Equal(string.Empty, Grader.Normalize(""));
    }
}
