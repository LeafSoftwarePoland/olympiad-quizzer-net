using OlympiadQuizzer.App.Api.Extensions;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L0.Extensions;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class CorsExtensionsTests
{
    private const string _configuredOrigin = "https://pages.example.com";

    [Theory]
    [InlineData("https://pages.example.com")]
    [InlineData("https://PAGES.EXAMPLE.COM")]
    [InlineData("http://localhost:5001")]
    [InlineData("http://127.0.0.1:5000")]
    public void IsAllowedOrigin_ReturnsTrue_WhenOriginIsAllowed(string origin) =>
        Assert.True(CorsExtensions.IsAllowedOrigin(origin, _configuredOrigin));

    [Theory]
    [InlineData("https://evil.example.com")]
    [InlineData("https://pages.example.com.evil.example")]
    [InlineData("not-a-valid-origin")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsAllowedOrigin_ReturnsFalse_WhenOriginIsNotAllowed(string origin) =>
        Assert.False(CorsExtensions.IsAllowedOrigin(origin, _configuredOrigin));

    [Theory]
    [InlineData("http://localhost:5001")]
    [InlineData("http://127.0.0.1:5000")]
    public void IsAllowedOrigin_ReturnsTrue_ForLocalhost_WhenConfiguredOriginIsEmpty(string origin) =>
        Assert.True(CorsExtensions.IsAllowedOrigin(origin, string.Empty));

    [Fact]
    public void IsAllowedOrigin_ReturnsFalse_ForConfiguredOrigin_WhenConfiguredOriginIsEmpty() =>
        Assert.False(CorsExtensions.IsAllowedOrigin("https://pages.example.com", string.Empty));
}
