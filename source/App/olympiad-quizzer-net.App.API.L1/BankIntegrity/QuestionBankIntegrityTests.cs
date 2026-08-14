using System.Text.Json;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.App.Api.L1.BankIntegrity;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class QuestionBankIntegrityTests
{
    [Fact]
    public void ProductionBank_AllQuestions_DoesNotContainUnknownType()
    {
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        Assert.Equal(210, questions.Count);

        using JsonDocument doc = JsonDocument.Parse(json);

        List<string> violations = [];
        foreach (JsonElement element in doc.RootElement.EnumerateArray())
        {
            int id = element.GetProperty("id").GetInt32();
            string rawType = element.TryGetProperty("type", out JsonElement typeProp)
                ? typeProp.GetString() ?? "(null)"
                : "(missing)";

            Question question = questions.Find(q => q.Id == id);
            if (question != null && question.Type == QuestionType.Unknown)
            {
                violations.Add($"  id={id} type=\"{rawType}\"");
            }
        }

        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with QuestionType.Unknown:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }
}
