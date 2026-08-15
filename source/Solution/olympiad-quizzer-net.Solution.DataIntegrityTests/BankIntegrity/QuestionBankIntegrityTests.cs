using System.Globalization;
using System.Text;
using System.Text.Json;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Harness;

namespace OlympiadQuizzer.Solution.DataIntegrityTests.BankIntegrity;

[Trait(TestTiers.Tier, TestTiers.Integrity)]
public sealed class QuestionBankIntegrityTests
{
    private static readonly HashSet<string> _knownTypeValues =
        ["single", "multi", "shortAnswer", "trueFalse", "ordering", "matching"];

    private const int _detectorIdShortAnswerTwo   = 9901;
    private const int _detectorIdShortAnswerPunct = 9902;
    private const int _detectorIdHangul           = 9903;
    private const int _detectorIdMissingImage     = 9904;
    private const int _detectorIdUnknownType      = 9905;
    private const int _detectorIdAnswerNotInOpts  = 9906;
    private const int _detectorIdEmptyCategory    = 9907;
    private const int _detectorIdMissingAlt       = 9908;
    private const int _detectorIdUnknownTag       = 9909;

    #region Production bank assertions

    [Fact]
    public void ProductionBank_HasNoShortAnswerViolations_WhenAllQuestionsChecked()
    {
        // Arrange
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        // Act
        List<string> violations = CollectShortAnswerViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} shortAnswer question(s) with invalid correctAnswer:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoHangulCodepoints_WhenAllQuestionsChecked()
    {
        // Arrange
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        // Act
        List<string> violations = CollectHangulViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with Hangul (U+AC00–U+D7A3):{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoMissingImages_WhenAllReferencedImagesChecked()
    {
        // Arrange
        string repoRoot  = FixturePath.RepoRoot();
        string bankPath  = Path.Combine(repoRoot, "data", "questions.json");
        string imagesDir = Path.Combine(repoRoot, "data", "images");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        // Act
        List<string> violations = CollectImageViolations(questions, imagesDir);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} missing image(s):{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoUnknownTypes_WhenRawJsonVocabularyChecked()
    {
        // Arrange
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string rawJson = File.ReadAllText(bankPath);

        // Act
        List<string> violations = CollectTypeVocabularyViolations(rawJson);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with unrecognised type:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoAnswerOutsideOptions_WhenAllClosedListQuestionsChecked()
    {
        // Arrange
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        // Act
        List<string> violations = CollectAnswerOptionViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) where correctAnswer is not among options after normalisation:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoCategoryViolations_WhenAllQuestionsChecked()
    {
        // Arrange
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        // Act
        List<string> violations = CollectCategoryViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with missing or empty category:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoImageBlocksWithoutAlt_WhenAllQuestionsChecked()
    {
        // Arrange
        string bankPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
        string json = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        // Act
        List<string> violations = CollectAltViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} image block(s) without mandatory Polish alt text:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoUnknownTagValues_WhenAllTagsCheckedAgainstVocabulary()
    {
        // Arrange
        string repoRoot  = FixturePath.RepoRoot();
        string bankPath  = Path.Combine(repoRoot, "data", "questions.json");
        string tagsPath  = Path.Combine(repoRoot, "docs", "tags.md");
        string json      = File.ReadAllText(bankPath);
        List<Question> questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);
        HashSet<string> validTags = ParseTagVocabulary(tagsPath);

        // Act
        List<string> violations = CollectTagViolations(questions, validTags);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with tag values not in docs/tags.md:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    #endregion

    #region Detector assertions

    [Fact]
    public void ShortAnswerDetector_ReportsViolation_WhenCorrectAnswerHasTwoItems()
    {
        // Arrange
        Question bad = new()
        {
            Id            = _detectorIdShortAnswerTwo,
            Type          = QuestionType.ShortAnswer,
            CorrectAnswer = ["answer-one", "answer-two"]
        };

        // Act
        List<string> violations = CollectShortAnswerViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdShortAnswerTwo.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void ShortAnswerDetector_ReportsViolation_WhenAnswerEndsWithPunctuationMark()
    {
        // Arrange
        Question bad = new()
        {
            Id            = _detectorIdShortAnswerPunct,
            Type          = QuestionType.ShortAnswer,
            CorrectAnswer = ["Is it?"]
        };

        // Act
        List<string> violations = CollectShortAnswerViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdShortAnswerPunct.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void HangulDetector_ReportsViolation_WhenQuestionTextContainsKoreanCharacters()
    {
        // Arrange
        const string koreanSyllables = "가각";
        Question bad = new()
        {
            Id      = _detectorIdHangul,
            Type    = QuestionType.Single,
            Content = [new ContentBlock { Type = ContentBlockType.Text, Text = koreanSyllables }]
        };

        // Act
        List<string> violations = CollectHangulViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdHangul.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void ImageDetector_ReportsViolation_WhenReferencedFileDoesNotExist()
    {
        // Arrange
        const string missingFile = "this-image-does-not-exist-9904.png";
        string imagesDir = Path.Combine(FixturePath.RepoRoot(), "data", "images");
        Question bad = new()
        {
            Id      = _detectorIdMissingImage,
            Type    = QuestionType.Single,
            Content = [new ContentBlock { Type = ContentBlockType.Image, File = missingFile }]
        };

        // Act
        List<string> violations = CollectImageViolations([bad], imagesDir);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdMissingImage.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void TypeVocabularyChecker_ReportsViolation_WhenTypeStringIsUnrecognised()
    {
        // Arrange
        const int unknownTypeId = _detectorIdUnknownType;
        string badJson = $"[{{\"id\": {unknownTypeId}, \"type\": \"unknownType\", \"content\": [], \"category\": [], \"olympiad\": \"OIJ\", \"stage\": \"E1\", \"correctAnswer\": \"x\"}}]";

        // Act
        List<string> violations = CollectTypeVocabularyViolations(badJson);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(unknownTypeId.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerOptionsChecker_ReportsViolation_WhenCorrectAnswerIsNotInOptions()
    {
        // Arrange
        Question bad = new()
        {
            Id            = _detectorIdAnswerNotInOpts,
            Type          = QuestionType.Single,
            Options       = ["Option A", "Option B"],
            CorrectAnswer = ["Option C"]
        };

        // Act
        List<string> violations = CollectAnswerOptionViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdAnswerNotInOpts.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void CategoryChecker_ReportsViolation_WhenCategoryIsEmpty()
    {
        // Arrange
        Question bad = new()
        {
            Id       = _detectorIdEmptyCategory,
            Type     = QuestionType.Single,
            Category = []
        };

        // Act
        List<string> violations = CollectCategoryViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdEmptyCategory.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AltChecker_ReportsViolation_WhenImageBlockHasNoAlt()
    {
        // Arrange
        Question bad = new()
        {
            Id      = _detectorIdMissingAlt,
            Type    = QuestionType.Single,
            Content = [new ContentBlock { Type = ContentBlockType.Image, File = "diagram.png", Alt = null }]
        };

        // Act
        List<string> violations = CollectAltViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdMissingAlt.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void TagChecker_ReportsViolation_WhenCategoryTagIsNotInVocabulary()
    {
        // Arrange
        HashSet<string> knownTags = ["sledzenie_kodu", "rekurencja"];
        Question bad = new()
        {
            Id       = _detectorIdUnknownTag,
            Type     = QuestionType.Single,
            Category = ["sledzenie_kodu", "invalid_category_xyz"]
        };

        // Act
        List<string> violations = CollectTagViolations([bad], knownTags);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdUnknownTag.ToString(), string.Join(" ", violations));
    }

    #endregion

    #region Violation collectors

    private static List<string> CollectShortAnswerViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            if (question.Type != QuestionType.ShortAnswer)
                continue;

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
                allBlocks.AddRange(question.Content);
            if (question.ContentCpp != null)
                allBlocks.AddRange(question.ContentCpp);
            if (question.Explanation != null)
                allBlocks.AddRange(question.Explanation);

            foreach (ContentBlock block in allBlocks)
            {
                if (block.Type != ContentBlockType.Image)
                    continue;

                if (string.IsNullOrEmpty(block.File))
                {
                    violations.Add($"  id={question.Id}: image block has no File specified");
                    continue;
                }

                string imagePath = Path.Combine(imagesDir, block.File);
                if (!File.Exists(imagePath))
                    violations.Add($"  id={question.Id}: image not found at data/images/{block.File}");
            }
        }

        return violations;
    }

    private static List<string> CollectTypeVocabularyViolations(string rawJson)
    {
        List<string> violations = [];

        using JsonDocument doc = JsonDocument.Parse(rawJson);
        foreach (JsonElement element in doc.RootElement.EnumerateArray())
        {
            int questionId = element.TryGetProperty("id", out JsonElement idProp)
                ? idProp.GetInt32()
                : -1;

            string typeValue = element.TryGetProperty("type", out JsonElement typeProp)
                ? typeProp.GetString()
                : null;

            if (!_knownTypeValues.Contains(typeValue ?? string.Empty))
            {
                string displayType = EscapeInvisible(typeValue ?? "(missing)");
                violations.Add($"  id={questionId}: unrecognised type \"{displayType}\"");
            }
        }

        return violations;
    }

    private static List<string> CollectAnswerOptionViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            List<string> optionPool = question.Type switch
            {
                QuestionType.Matching  => question.MatchOptions ?? [],
                QuestionType.Single
                    or QuestionType.Multi
                    or QuestionType.Ordering => question.Options ?? [],
                _ => null
            };

            // ShortAnswer has no options pool. TrueFalse uses positional boolean markers
            // ("true"/"false"), not option text, so the standard invariant does not apply.
            if (optionPool == null)
                continue;

            HashSet<string> normalizedPool = new(
                optionPool.Select(Normalization.NormalizeChoice),
                StringComparer.Ordinal);

            foreach (string answer in question.CorrectAnswer ?? [])
            {
                string normalizedAnswer = Normalization.NormalizeChoice(answer);
                if (!normalizedPool.Contains(normalizedAnswer))
                {
                    string displayAnswer = EscapeInvisible(answer);
                    violations.Add(
                        $"  id={question.Id} ({question.Type}): correctAnswer \"{displayAnswer}\" not found in options after normalisation");
                }
            }
        }

        return violations;
    }

    private static List<string> CollectCategoryViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            if (question.Category == null || question.Category.Count == 0)
                violations.Add($"  id={question.Id}: category is missing or empty");
        }

        return violations;
    }

    private static List<string> CollectAltViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            foreach (List<ContentBlock> blockList in new[]
            {
                question.Content,
                question.ContentCpp,
                question.Explanation
            })
            {
                if (blockList == null)
                    continue;

                foreach (ContentBlock block in blockList)
                {
                    if (block.Type == ContentBlockType.Image && string.IsNullOrEmpty(block.Alt))
                    {
                        string fileName = block.File ?? "(none)";
                        violations.Add(
                            $"  id={question.Id}: image block (file={fileName}) has no alt text");
                    }
                }
            }
        }

        return violations;
    }

    private static List<string> CollectTagViolations(List<Question> questions, HashSet<string> validTags)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            foreach (string tag in question.Category ?? [])
            {
                if (!validTags.Contains(tag))
                    violations.Add($"  id={question.Id}: category tag \"{EscapeInvisible(tag)}\" not in docs/tags.md");
            }

            foreach (string tag in question.Algorithms ?? [])
            {
                if (!validTags.Contains(tag))
                    violations.Add($"  id={question.Id}: algorithms tag \"{EscapeInvisible(tag)}\" not in docs/tags.md");
            }
        }

        return violations;
    }

    private static HashSet<string> ParseTagVocabulary(string tagsPath)
    {
        var tags = new HashSet<string>(StringComparer.Ordinal);
        string[] lines = File.ReadAllLines(tagsPath);

        foreach (string line in lines)
        {
            if (!line.StartsWith("| `"))
                continue;

            int closeBacktick = line.IndexOf('`', 3);
            if (closeBacktick < 0)
                continue;

            string tag = line[3..closeBacktick];
            if (tag.Length > 0)
                tags.Add(tag);
        }

        return tags;
    }

    #endregion

    #region Helpers

    private static bool ContainsHangul(string text)
    {
        if (text == null)
            return false;

        foreach (char c in text)
        {
            if (c >= '가' && c <= '힣')
                return true;
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
            texts.AddRange(question.Options);

        if (question.MatchOptions != null)
            texts.AddRange(question.MatchOptions);

        if (question.CorrectAnswer != null)
            texts.AddRange(question.CorrectAnswer);

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
            return text ?? string.Empty;

        return text[..maxLength] + "…";
    }

    private static string EscapeInvisible(string value)
    {
        if (value == null)
            return "(null)";

        var sb = new StringBuilder(value.Length * 2);
        foreach (char c in value)
        {
            if (char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.OtherNotAssigned)
                sb.Append($"\\u{(int)c:X4}");
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    #endregion
}
