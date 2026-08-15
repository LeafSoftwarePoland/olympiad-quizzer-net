using OlympiadQuizzer.App.Api.Extensions;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L1.Extensions;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class CorsExtensionsTests
{
    [Fact]
    public void IsAllowedOrigin_WithGitHubPagesOrigin_ReturnsTrue()
    {
        bool result = CorsExtensions.IsAllowedOrigin("https://leafsoftwarepoland.github.io");

        Assert.True(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithGitHubPagesOriginUpperCase_ReturnsTrue()
    {
        bool result = CorsExtensions.IsAllowedOrigin("https://LEAFSOFTWAREPOLAND.GITHUB.IO");

        Assert.True(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithLocalhostAndPort_ReturnsTrue()
    {
        bool result = CorsExtensions.IsAllowedOrigin("http://localhost:5001");

        Assert.True(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithLoopbackIpAddress_ReturnsTrue()
    {
        bool result = CorsExtensions.IsAllowedOrigin("http://127.0.0.1:5000");

        Assert.True(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithUnknownOrigin_ReturnsFalse()
    {
        bool result = CorsExtensions.IsAllowedOrigin("https://evil.example.com");

        Assert.False(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithLookalikeOrigin_ReturnsFalse()
    {
        bool result = CorsExtensions.IsAllowedOrigin("https://leafsoftwarepoland.github.io.evil.example");

        Assert.False(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithMalformedOrigin_ReturnsFalse()
    {
        bool result = CorsExtensions.IsAllowedOrigin("not-a-valid-origin");

        Assert.False(result);
    }

    [Fact]
    public void IsAllowedOrigin_WithNullOrEmpty_ReturnsFalse()
    {
        Assert.False(CorsExtensions.IsAllowedOrigin(null));
        Assert.False(CorsExtensions.IsAllowedOrigin(""));
        Assert.False(CorsExtensions.IsAllowedOrigin("   "));
    }
}
