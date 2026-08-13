using System.Text.Json;
using OlympiadQuizzer.Domain.Serialization;

namespace OlympiadQuizzer.Domain.L0.Serialization;

[Trait("Tier", "L0")]
public sealed class StringOrStringArrayConverterTests
{
    private static readonly JsonSerializerOptions OptionsWithConverter = BuildOptions();

    [Fact]
    public void Read_WithBareJsonString_ReturnsSingleElementList()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("\"hello\"", OptionsWithConverter);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("hello", result[0]);
    }

    [Fact]
    public void Read_WithJsonArray_ReturnsAllElements()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[\"a\",\"b\",\"c\"]", OptionsWithConverter);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public void Read_WithEmptyArray_ReturnsEmptyList()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[]", OptionsWithConverter);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Read_WithJsonNull_ReturnsEmptyList()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("null", OptionsWithConverter);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Read_WithNumericArrayElement_CoercesToInvariantString()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[0,1]", OptionsWithConverter);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("0", result[0]);
        Assert.Equal("1", result[1]);
    }

    [Fact]
    public void Read_WithBooleanArrayElements_CoercesToTrueFalseStrings()
    {
        List<string> result = JsonSerializer.Deserialize<List<string>>("[true,false]", OptionsWithConverter);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("true", result[0]);
        Assert.Equal("false", result[1]);
    }

    [Fact]
    public void Read_WithNestedObject_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<List<string>>("[{\"key\":\"val\"}]", OptionsWithConverter));
    }

    [Fact]
    public void Write_WithSingleElement_EmitsBareString()
    {
        List<string> value = new List<string> { "answer" };

        string json = JsonSerializer.Serialize(value, OptionsWithConverter);

        Assert.Equal("\"answer\"", json);
    }

    [Fact]
    public void Write_WithMultipleElements_EmitsArray()
    {
        List<string> value = new List<string> { "a", "b" };

        string json = JsonSerializer.Serialize(value, OptionsWithConverter);

        Assert.Equal("[\"a\",\"b\"]", json);
    }

    [Fact]
    public void Write_WithEmptyList_EmitsEmptyArray()
    {
        List<string> value = new List<string>();

        string json = JsonSerializer.Serialize(value, OptionsWithConverter);

        Assert.Equal("[]", json);
    }

    [Fact]
    public void RoundTrip_BareStringThroughReadAndWrite_PreservesBareStringShape()
    {
        // Value chosen without HTML-unsafe characters so the test encoder does not escape them.
        // The purpose is to verify that a single-element list round-trips as a bare string, not an array.
        string original = "\"correct answer\"";

        List<string> deserialized = JsonSerializer.Deserialize<List<string>>(original, OptionsWithConverter);
        string serialized = JsonSerializer.Serialize(deserialized, OptionsWithConverter);

        Assert.Equal(original, serialized);
    }

    private static JsonSerializerOptions BuildOptions()
    {
        JsonSerializerOptions opts = new JsonSerializerOptions();
        opts.Converters.Add(new StringOrStringArrayConverter());
        return opts;
    }
}
