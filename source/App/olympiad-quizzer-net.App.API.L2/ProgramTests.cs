using Microsoft.AspNetCore.Mvc.Testing;
using OlympiadQuizzer.Core.Tests.Common;

namespace OlympiadQuizzer.App.Api.L2;

[Trait(TestTiers.Tier, TestTiers.L2)]
public sealed class ProgramTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public void CreateClient_ReturnsHttpClient_WhenApplicationBuildsSuccessfully() =>
        Assert.NotNull(factory.CreateClient());
}
