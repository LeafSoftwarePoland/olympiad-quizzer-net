using OlympiadQuizzer.Domain.Grading;

namespace OlympiadQuizzer.Domain.L0.Grading;

[Trait("Tier", "L0")]
public sealed class NormalizationTests
{
    // NormalizeChoice — closed-list normaliser: FormC, trim, lowercase, internal whitespace preserved

    // Decomposed form: z (U+007A) + combining dot above (U+0307).
    // Escape sequence is mandatory — an editor would silently compose the raw bytes.
    private const string DecomposedZWithDotAbove = "z\u0307";
    // Composed form: U+017C LATIN SMALL LETTER Z WITH DOT ABOVE
    private const string ComposedZWithDotAbove = "\u017c";

    // U+2081 SUBSCRIPT ONE, U+2086 SUBSCRIPT SIX — NormalizeChoice must not fold them
    private const string SubscriptSixteen = "AF\u2081\u2086";
    private const string SubscriptSixteenLower = "af\u2081\u2086";

    // NormalizeFreeText — free-text normaliser: FormKC folds compatibility characters

    // U+00B2 SUPERSCRIPT TWO, U+2076 SUPERSCRIPT SIX — FormKC folds both to ASCII digits
    private const string TwoSuperscriptTwoSix = "2\u00b2\u2076";
    // U+1D465 MATHEMATICAL ITALIC SMALL X — FormKC folds to ASCII x
    private const string MathItalicX = "\U0001D465";
    // U+00A0 NO-BREAK SPACE between a and b — FormKC maps it to ordinary space U+0020
    private const string NonBreakingSpaceBetweenAB = "a\u00a0b";
    // Polish diacritics (precomposed lowercase): U+017C z with dot, U+00F3 o with acute, U+0142 l with stroke
    private const string PolishWordZolty = "\u017c\u00f3\u0142ty";
    // What over-broad folding would produce (must NOT equal the normalised output)
    private const string PolishWordZoltyAscii = "zolty";

    [Fact]
    public void NormalizeChoice_WithNull_ReturnsEmptyString()
    {
        string result = Grader.NormalizeChoice(null);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeChoice_WithMixedCase_ReturnsLowercase()
    {
        string result = Grader.NormalizeChoice("ABC");

        Assert.Equal("abc", result);
    }

    [Fact]
    public void NormalizeChoice_WithLeadingAndTrailingWhitespace_ReturnsTrimmed()
    {
        string result = Grader.NormalizeChoice("  hello  ");

        Assert.Equal("hello", result);
    }

    [Fact]
    public void NormalizeChoice_WithDecomposedPolishDiacritic_ReturnsComposedForm()
    {
        string result = Grader.NormalizeChoice(DecomposedZWithDotAbove);

        Assert.Equal(ComposedZWithDotAbove, result);
    }

    [Fact]
    public void NormalizeChoice_WithSubscriptDigit_DoesNotFoldIt()
    {
        string result = Grader.NormalizeChoice(SubscriptSixteen);

        Assert.Equal(SubscriptSixteenLower, result);
    }

    [Fact]
    public void NormalizeChoice_WithInternalDoubleSpace_PreservesIt()
    {
        string result = Grader.NormalizeChoice("a  b");

        Assert.Equal("a  b", result);
    }

    // NormalizeFreeText tests

    [Fact]
    public void NormalizeFreeText_WithNull_ReturnsEmptyString()
    {
        string result = Grader.NormalizeFreeText(null);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeFreeText_WithSubscriptDigits_FoldsToAscii()
    {
        string result = Grader.NormalizeFreeText(SubscriptSixteen);

        Assert.Equal("af16", result);
    }

    [Fact]
    public void NormalizeFreeText_WithSuperscriptDigits_FoldsToAscii()
    {
        string result = Grader.NormalizeFreeText(TwoSuperscriptTwoSix);

        Assert.Equal("226", result);
    }

    [Fact]
    public void NormalizeFreeText_WithMathematicalItalic_FoldsToLatin()
    {
        string result = Grader.NormalizeFreeText(MathItalicX);

        Assert.Equal("x", result);
    }

    [Fact]
    public void NormalizeFreeText_WithMultipleInternalSpaces_CollapsesToSingleSpace()
    {
        string result = Grader.NormalizeFreeText("a   b");

        Assert.Equal("a b", result);
    }

    [Fact]
    public void NormalizeFreeText_WithNonBreakingSpace_TreatsItAsSpace()
    {
        string result = Grader.NormalizeFreeText(NonBreakingSpaceBetweenAB);

        Assert.Equal("a b", result);
    }

    [Fact]
    public void NormalizeFreeText_WithPolishDiacritics_PreservesThem()
    {
        string result = Grader.NormalizeFreeText(PolishWordZolty);

        Assert.Equal(PolishWordZolty, result);
        Assert.NotEqual(PolishWordZoltyAscii, result);
    }
}
