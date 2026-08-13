using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Json;

namespace OlympiadQuizzer.Api.L1.Infrastructure;

[Trait("Tier", "L1")]
public sealed class QuestionBankLoaderTests
{
    private const string FilteringBank     = "filtering-bank.json";
    private const string SingleBank        = "single-question-bank.json";
    private const string EmptyBank         = "empty-bank.json";
    private const string MalformedBank     = "malformed-bank.json";
    private const string NullElementBank   = "null-element-bank.json";

    [Fact]
    public void Constructor_WithValidBankFile_LoadsAllQuestions()
    {
        QuestionBankLoader loader = RepositoryHarness.Loader(FilteringBank);

        Assert.Equal(15, loader.Bank.Questions.Count);
    }

    [Fact]
    public void Constructor_WithRelativePath_ResolvesAgainstBaseDirectory()
    {
        string relativePath = Path.Combine("Fixtures", FilteringBank);
        QuestionBankLoader loader = RepositoryHarness.LoaderForPath(
            relativePath, Microsoft.Extensions.Logging.Abstractions.NullLogger<QuestionBankLoader>.Instance);

        Assert.Equal(15, loader.Bank.Questions.Count);
    }

    [Fact]
    public void Constructor_WithMissingFile_ThrowsFileNotFoundException()
    {
        string missingPath = FixturePath.Resolve("no-such-bank.json");

        Assert.Throws<FileNotFoundException>(() => RepositoryHarness.LoaderForPath(
            missingPath, Microsoft.Extensions.Logging.Abstractions.NullLogger<QuestionBankLoader>.Instance));
    }

    [Fact]
    public void Constructor_WithEmptyArrayBank_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => RepositoryHarness.Loader(EmptyBank));
    }

    [Fact]
    public void Constructor_WithMalformedJson_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => RepositoryHarness.Loader(MalformedBank));
    }

    [Fact]
    public void Constructor_WithUtf8BomPrefixedFile_LoadsSuccessfully()
    {
        string source = File.ReadAllText(FixturePath.Resolve(SingleBank));
        string temp = Path.Combine(Path.GetTempPath(), "oqn-bom-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(temp, source, new UTF8Encoding(true));

            Assert.Equal(0xEF, File.ReadAllBytes(temp)[0]);

            QuestionBankLoader loader = RepositoryHarness.LoaderForPath(
                temp, Microsoft.Extensions.Logging.Abstractions.NullLogger<QuestionBankLoader>.Instance);

            Assert.Single(loader.Bank.Questions);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void Constructor_LogsQuestionCountAtInformation()
    {
        CapturingLogger<QuestionBankLoader> logger = new CapturingLogger<QuestionBankLoader>();

        RepositoryHarness.Loader(FilteringBank, logger);

        Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logger.Entries[0].Level);
        Assert.Contains("15", logger.Entries[0].Message);
        Assert.Contains("filtering-bank.json", logger.Entries[0].Message);
    }

    [Fact]
    public void Constructor_WithNullQuestionEntry_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => RepositoryHarness.Loader(NullElementBank));
    }
}
