using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class NormalizationTests
{
    // NormalizeChoice — closed-list normaliser: FormC, trim, lowercase, internal whitespace preserved

    // Decomposed form: z (U+007A) + combining dot above (U+0307).
    // Escape sequence is mandatory — an editor would silently compose the raw bytes.
    private const string _decomposedZWithDotAbove = "z\u0307";
    // Composed form: U+017C LATIN SMALL LETTER Z WITH DOT ABOVE
    private const string _composedZWithDotAbove = "\u017c";

    // U+2081 SUBSCRIPT ONE, U+2086 SUBSCRIPT SIX — NormalizeChoice must not fold them
    private const string _subscriptSixteen = "AF\u2081\u2086";
    private const string _subscriptSixteenLower = "af\u2081\u2086";

    // NormalizeFreeText — free-text normaliser: FormKC folds compatibility characters

    // U+00B2 SUPERSCRIPT TWO, U+2076 SUPERSCRIPT SIX — FormKC folds both to ASCII digits
    private const string _twoSuperscriptTwoSix = "2\u00b2\u2076";
    // U+1D465 MATHEMATICAL ITALIC SMALL X — FormKC folds to ASCII x
    private const string _mathItalicX = "\U0001D465";
    // U+00A0 NO-BREAK SPACE between a and b — FormKC maps it to ordinary space U+0020
    private const string _nonBreakingSpaceBetweenAB = "a\u00a0b";
    // Polish diacritics (precomposed lowercase): U+017C z with dot, U+00F3 o with acute, U+0142 l with stroke
    private const string _polishWordZolty = "\u017c\u00f3\u0142ty";
    // What over-broad folding would produce (must NOT equal the normalised output)
    private const string _polishWordZoltyAscii = "zolty";

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
        string result = Grader.NormalizeChoice(_decomposedZWithDotAbove);

        Assert.Equal(_composedZWithDotAbove, result);
    }

    [Fact]
    public void NormalizeChoice_WithSubscriptDigit_DoesNotFoldIt()
    {
        string result = Grader.NormalizeChoice(_subscriptSixteen);

        Assert.Equal(_subscriptSixteenLower, result);
    }

    [Fact]
    public void NormalizeChoice_WithInternalDoubleSpace_DoesPreserveIt()
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
    public void NormalizeFreeText_WithSubscriptDigits_ReturnsAsciiEquivalent()
    {
        string result = Grader.NormalizeFreeText(_subscriptSixteen);

        Assert.Equal("af16", result);
    }

    [Fact]
    public void NormalizeFreeText_WithSuperscriptDigits_ReturnsAsciiEquivalent()
    {
        string result = Grader.NormalizeFreeText(_twoSuperscriptTwoSix);

        Assert.Equal("226", result);
    }

    [Fact]
    public void NormalizeFreeText_WithMathematicalItalic_ReturnsLatinEquivalent()
    {
        string result = Grader.NormalizeFreeText(_mathItalicX);

        Assert.Equal("x", result);
    }

    [Fact]
    public void NormalizeFreeText_WithMultipleInternalSpaces_ReturnsSingleSpace()
    {
        string result = Grader.NormalizeFreeText("a   b");

        Assert.Equal("a b", result);
    }

    [Fact]
    public void NormalizeFreeText_WithNonBreakingSpace_ReturnsOrdinarySpace()
    {
        string result = Grader.NormalizeFreeText(_nonBreakingSpaceBetweenAB);

        Assert.Equal("a b", result);
    }

    [Fact]
    public void NormalizeFreeText_WithPolishDiacritics_DoesPreserveThem()
    {
        string result = Grader.NormalizeFreeText(_polishWordZolty);

        Assert.Equal(_polishWordZolty, result);
        Assert.NotEqual(_polishWordZoltyAscii, result);
    }
}
