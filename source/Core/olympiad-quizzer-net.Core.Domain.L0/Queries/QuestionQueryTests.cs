using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Queries;

namespace OlympiadQuizzer.Core.Domain.L0.Queries;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class QuestionQueryTests
{
    [Fact]
    public void MaxLimit_IsThirty()
    {
        Assert.Equal(30, QuestionQuery.MaxLimit);
    }

    [Fact]
    public void DefaultLimit_Always_IsMaxLimit()
    {
        Assert.Equal(QuestionQuery.MaxLimit, QuestionQuery.DefaultLimit);
    }

    [Fact]
    public void NewQuery_Default_ReturnsEmptyNonNullCollections()
    {
        QuestionQuery query = new();

        Assert.NotNull(query.Categories);
        Assert.NotNull(query.Algorithms);
        Assert.NotNull(query.Years);
        Assert.NotNull(query.Stages);
        Assert.Empty(query.Categories);
        Assert.Empty(query.Algorithms);
        Assert.Empty(query.Years);
        Assert.Empty(query.Stages);
    }

    [Fact]
    public void HasAnyFilter_WithNoValues_ReturnsFalse()
    {
        QuestionQuery query = new();

        Assert.False(query.HasAnyFilter);
    }

    [Fact]
    public void HasAnyFilter_WithOnlyYears_ReturnsTrue()
    {
        QuestionQuery query = new();
        query.Years.Add(2024);

        Assert.True(query.HasAnyFilter);
    }
}
