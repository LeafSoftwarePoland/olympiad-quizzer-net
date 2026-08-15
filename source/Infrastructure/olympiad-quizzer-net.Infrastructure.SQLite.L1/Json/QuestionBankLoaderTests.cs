using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Json;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class QuestionBankLoaderTests
{
    private const string _filteringBank   = "filtering-bank.json";
    private const string _singleBank      = "single-question-bank.json";
    private const string _emptyBank       = "empty-bank.json";
    private const string _malformedBank   = "malformed-bank.json";
    private const string _nullElementBank = "null-element-bank.json";

    [Fact]
    public void Constructor_WithValidBankFile_DoesLoadAllQuestions()
    {
        QuestionBankLoader loader = RepositoryHarness.Loader(_filteringBank);

        Assert.Equal(15, loader.Bank.Questions.Count);
    }

    [Fact]
    public void Constructor_WithRelativePath_DoesResolveAgainstBaseDirectory()
    {
        string relativePath = Path.Combine("Fixtures", _filteringBank);
        QuestionBankLoader loader = RepositoryHarness.LoaderForPath(
            relativePath, NullLogger<QuestionBankLoader>.Instance);

        Assert.Equal(15, loader.Bank.Questions.Count);
    }

    [Fact]
    public void Constructor_WithMissingFile_ThrowsFileNotFoundException()
    {
        string missingPath = FixturePath.Resolve("no-such-bank.json");

        Assert.Throws<FileNotFoundException>(() => RepositoryHarness.LoaderForPath(
            missingPath, NullLogger<QuestionBankLoader>.Instance));
    }

    [Fact]
    public void Constructor_WithEmptyArrayBank_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => RepositoryHarness.Loader(_emptyBank));
    }

    [Fact]
    public void Constructor_WithMalformedJson_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => RepositoryHarness.Loader(_malformedBank));
    }

    [Fact]
    public void Constructor_WithUtf8BomPrefixedFile_DoesLoadSuccessfully()
    {
        string source = File.ReadAllText(FixturePath.Resolve(_singleBank));
        string temp = Path.Combine(Path.GetTempPath(), "oqn-bom-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(temp, source, new UTF8Encoding(true));

            Assert.Equal(0xEF, File.ReadAllBytes(temp)[0]);

            QuestionBankLoader loader = RepositoryHarness.LoaderForPath(
                temp, NullLogger<QuestionBankLoader>.Instance);

            Assert.Single(loader.Bank.Questions);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void Constructor_ValidBank_DoesLogQuestionCountAtInformation()
    {
        CapturingLogger<QuestionBankLoader> logger = new();

        RepositoryHarness.Loader(_filteringBank, logger);

        Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logger.Entries[0].Level);
        Assert.Contains("15", logger.Entries[0].Message);
        Assert.Contains("filtering-bank.json", logger.Entries[0].Message);
    }

    [Fact]
    public void Constructor_WithNullQuestionEntry_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => RepositoryHarness.Loader(_nullElementBank));
    }
}
