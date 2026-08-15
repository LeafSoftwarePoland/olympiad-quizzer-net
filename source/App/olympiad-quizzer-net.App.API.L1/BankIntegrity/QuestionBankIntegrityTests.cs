using System.Text.Json;
using OlympiadQuizzer.App.Api.L1.Harness;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.App.Api.L1.BankIntegrity;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class QuestionBankIntegrityTests
{
    // ────────────────────────────────────────────────────────────────
    //  Production-bank assertions
    // ────────────────────────────────────────────────────────────────

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

    [Fact]
    public void ProductionBank_ShortAnswerQuestions_EachHasExactlyOneNonEmptyAnswerNotEndingWithPunctuationMark()
    {
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        List<string> violations = CollectShortAnswerViolations(questions);

        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} shortAnswer question(s) with invalid correctAnswer:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_NoQuestion_ContainsHangulCodepoints()
    {
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        List<string> violations = CollectHangulViolations(questions);

        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with Hangul (U+AC00–U+D7A3):{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_AllReferencedImages_ExistInImagesDirectory()
    {
        string repoRoot = FixturePath.RepoRoot();
        string bankPath  = Path.Combine(repoRoot, "data", "questions.json");
        string imagesDir = Path.Combine(repoRoot, "data", "images");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        List<string> violations = CollectImageViolations(questions, imagesDir);

        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} missing image(s):{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    // ────────────────────────────────────────────────────────────────
    //  Detector-proof tests — verify each assertion catches violations
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public void ShortAnswerDetector_WhenCorrectAnswerHasTwoItems_ReportsViolation()
    {
        Question bad = new()
        {
            Id   = 9901,
            Type = QuestionType.ShortAnswer,
            CorrectAnswer = ["answer-one", "answer-two"]
        };

        List<string> violations = CollectShortAnswerViolations([bad]);

        Assert.NotEmpty(violations);
        Assert.Contains("9901", string.Join(" ", violations));
    }

    [Fact]
    public void ShortAnswerDetector_WhenAnswerEndsWithQuestionMark_ReportsViolation()
    {
        Question bad = new()
        {
            Id   = 9902,
            Type = QuestionType.ShortAnswer,
            CorrectAnswer = ["Is it?"]
        };

        List<string> violations = CollectShortAnswerViolations([bad]);

        Assert.NotEmpty(violations);
        Assert.Contains("9902", string.Join(" ", violations));
    }

    [Fact]
    public void HangulDetector_WhenQuestionTextContainsKoreanCharacters_ReportsViolation()
    {
        // "가" is the first Hangul syllable character (가)
        Question bad = new()
        {
            Id      = 9903,
            Type    = QuestionType.Single,
            Content = [new ContentBlock { Type = ContentBlockType.Text, Text = "가각" }]
        };

        List<string> violations = CollectHangulViolations([bad]);

        Assert.NotEmpty(violations);
        Assert.Contains("9903", string.Join(" ", violations));
    }

    [Fact]
    public void ImageDetector_WhenReferencedFileDoesNotExist_ReportsViolation()
    {
        string imagesDir = Path.Combine(FixturePath.RepoRoot(), "data", "images");

        Question bad = new()
        {
            Id      = 9904,
            Type    = QuestionType.Single,
            Content = [new ContentBlock { Type = ContentBlockType.Image, File = "this-image-does-not-exist-9904.png" }]
        };

        List<string> violations = CollectImageViolations([bad], imagesDir);

        Assert.NotEmpty(violations);
        Assert.Contains("9904", string.Join(" ", violations));
    }

    // ────────────────────────────────────────────────────────────────
    //  Shared detection helpers
    // ────────────────────────────────────────────────────────────────

    private static List<string> CollectShortAnswerViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            if (question.Type != QuestionType.ShortAnswer)
            {
                continue;
            }

            List<string> answers = question.CorrectAnswer ?? [];

            if (answers.Count != 1)
            {
                violations.Add($"  id={question.Id}: expected exactly 1 correctAnswer, found {answers.Count}");
                continue;
            }

            string answer = answers[0];

            if (string.IsNullOrEmpty(answer))
            {
                violations.Add($"  id={question.Id}: correctAnswer is empty");
                continue;
            }

            if (answer.EndsWith('?') || answer.EndsWith('.'))
            {
                violations.Add($"  id={question.Id}: correctAnswer ends with '?' or '.' — \"{answer}\"");
            }
        }

        return violations;
    }

    private static List<string> CollectHangulViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            List<string> textsToCheck = GatherTexts(question);
            foreach (string text in textsToCheck)
            {
                if (ContainsHangul(text))
                {
                    violations.Add($"  id={question.Id}: contains Hangul characters in text: \"{Truncate(text, 60)}\"");
                    break;
                }
            }
        }

        return violations;
    }

    private static List<string> CollectImageViolations(List<Question> questions, string imagesDir)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            List<ContentBlock> allBlocks = [];

            if (question.Content != null)
            {
                allBlocks.AddRange(question.Content);
            }
            if (question.ContentCpp != null)
            {
                allBlocks.AddRange(question.ContentCpp);
            }
            if (question.Explanation != null)
            {
                allBlocks.AddRange(question.Explanation);
            }

            foreach (ContentBlock block in allBlocks)
            {
                if (block.Type != ContentBlockType.Image)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(block.File))
                {
                    violations.Add($"  id={question.Id}: image block has no File specified");
                    continue;
                }

                string imagePath = Path.Combine(imagesDir, block.File);
                if (!File.Exists(imagePath))
                {
                    violations.Add($"  id={question.Id}: image not found at data/images/{block.File}");
                }
            }
        }

        return violations;
    }

    private static bool ContainsHangul(string text)
    {
        if (text == null)
        {
            return false;
        }
        foreach (char c in text)
        {
            if (c >= '가' && c <= '힣')
            {
                return true;
            }
        }
        return false;
    }

    private static List<string> GatherTexts(Question question)
    {
        List<string> texts = [];

        if (question.Content != null)
        {
            foreach (ContentBlock block in question.Content)
            {
                texts.Add(block.Text);
                texts.Add(block.Alt);
            }
        }

        if (question.ContentCpp != null)
        {
            foreach (ContentBlock block in question.ContentCpp)
            {
                texts.Add(block.Text);
            }
        }

        if (question.Options != null)
        {
            texts.AddRange(question.Options);
        }

        if (question.MatchOptions != null)
        {
            texts.AddRange(question.MatchOptions);
        }

        if (question.CorrectAnswer != null)
        {
            texts.AddRange(question.CorrectAnswer);
        }

        if (question.Explanation != null)
        {
            foreach (ContentBlock block in question.Explanation)
            {
                texts.Add(block.Text);
            }
        }

        return texts;
    }

    private static string Truncate(string text, int maxLength)
    {
        if (text == null || text.Length <= maxLength)
        {
            return text ?? string.Empty;
        }
        return text[..maxLength] + "…";
    }
}
