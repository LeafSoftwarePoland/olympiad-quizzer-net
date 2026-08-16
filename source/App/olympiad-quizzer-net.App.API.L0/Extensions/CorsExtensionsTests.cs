using OlympiadQuizzer.App.Api.Extensions;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L0.Extensions;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class CorsExtensionsTests
{
    [Theory]
    [InlineData("https://leafsoftwarepoland.github.io")]
    [InlineData("https://LEAFSOFTWAREPOLAND.GITHUB.IO")]
    [InlineData("http://localhost:5001")]
    [InlineData("http://127.0.0.1:5000")]
    public void IsAllowedOrigin_ReturnsTrue_WhenOriginIsAllowed(string origin) =>
        Assert.True(CorsExtensions.IsAllowedOrigin(origin));

    [Theory]
    [InlineData("https://evil.example.com")]
    [InlineData("https://leafsoftwarepoland.github.io.evil.example")]
    [InlineData("not-a-valid-origin")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsAllowedOrigin_ReturnsFalse_WhenOriginIsNotAllowed(string origin) =>
        Assert.False(CorsExtensions.IsAllowedOrigin(origin));
}
