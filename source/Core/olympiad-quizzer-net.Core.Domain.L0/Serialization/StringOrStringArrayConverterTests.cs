using System.Text.Json;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Core.Domain.L0.Serialization;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class StringOrStringArrayConverterTests
{
    private static readonly JsonSerializerOptions _optionsWithConverter = BuildOptions();

    [Fact]
    public void Read_WithBareJsonString_ReturnsSingleElementList()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("\"hello\"", _optionsWithConverter);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("hello", result[0]);
    }

    [Fact]
    public void Read_WithJsonArray_ReturnsAllElements()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[\"a\",\"b\",\"c\"]", _optionsWithConverter);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public void Read_WithEmptyArray_ReturnsEmptyList()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[]", _optionsWithConverter);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Read_WithJsonNull_ReturnsEmptyList()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("null", _optionsWithConverter);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Read_WithNumericArrayElement_ReturnsInvariantString()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[0,1]", _optionsWithConverter);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("0", result[0]);
        Assert.Equal("1", result[1]);
    }

    [Fact]
    public void Read_WithBooleanArrayElements_ReturnsTrueFalseStrings()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[true,false]", _optionsWithConverter);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("true", result[0]);
        Assert.Equal("false", result[1]);
    }

    [Fact]
    public void Read_WithNestedObject_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<List<string>>("[{\"key\":\"val\"}]", _optionsWithConverter));
    }

    [Fact]
    public void Write_WithSingleElement_ReturnsBareString()
    {
        List<string> value = ["answer"];

        string json = JsonSerializer.Serialize(value, _optionsWithConverter);

        Assert.Equal("\"answer\"", json);
    }

    [Fact]
    public void Write_WithMultipleElements_ReturnsArray()
    {
        List<string> value = ["a", "b"];

        string json = JsonSerializer.Serialize(value, _optionsWithConverter);

        Assert.Equal("[\"a\",\"b\"]", json);
    }

    [Fact]
    public void Write_WithEmptyList_ReturnsEmptyArray()
    {
        List<string> value = [];

        string json = JsonSerializer.Serialize(value, _optionsWithConverter);

        Assert.Equal("[]", json);
    }

    [Fact]
    public void RoundTrip_BareStringThroughReadAndWrite_DoesPreserveBareStringShape()
    {
        // Value chosen without HTML-unsafe characters so the test encoder does not escape them.
        // The purpose is to verify that a single-element list round-trips as a bare string, not an array.
        string original = "\"correct answer\"";

        List<string> deserialized = JsonSerializer.Deserialize<List<string>>(original, _optionsWithConverter);
        string serialized = JsonSerializer.Serialize(deserialized, _optionsWithConverter);

        Assert.Equal(original, serialized);
    }

    private static JsonSerializerOptions BuildOptions()
    {
        JsonSerializerOptions opts = new();
        opts.Converters.Add(new StringOrStringArrayConverter());
        return opts;
    }
}
