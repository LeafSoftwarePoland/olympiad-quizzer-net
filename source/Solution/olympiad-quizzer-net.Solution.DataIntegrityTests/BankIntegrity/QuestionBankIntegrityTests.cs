using System.Globalization;
using System.Text;
using System.Text.Json;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Builders;
using OlympiadQuizzer.Core.Tests.Common.Harness;

namespace OlympiadQuizzer.Solution.DataIntegrityTests.BankIntegrity;

[Trait(TestTiers.Tier, TestTiers.Integrity)]
public sealed class QuestionBankIntegrityTests
{
    private static readonly HashSet<string> _knownTypeValues =
        ["single", "multi", "shortAnswer", "trueFalse", "ordering", "matching"];

    private static readonly HashSet<string> _trueFalseTokens =
        ["true", "false"];

    private const int _detectorIdShortAnswerTwo   = 9901;
    private const int _detectorIdShortAnswerPunct = 9902;
    private const int _detectorIdHangul           = 9903;
    private const int _detectorIdMissingImage     = 9904;
    private const int _detectorIdUnknownType      = 9905;
    private const int _detectorIdAnswerNotInOpts  = 9906;
    private const int _detectorIdEmptyCategory    = 9907;
    private const int _detectorIdMissingAlt       = 9908;
    private const int _detectorIdUnknownTag       = 9909;
    private const int _detectorIdCrossAxisTag     = 9910;
    private const int _detectorIdMatchingAnswer   = 9911;
    private const int _detectorIdTrueFalseCount   = 9912;
    private const int _detectorIdTrueFalseToken   = 9913;
    private const int _detectorIdOrderingDupe     = 9914;
    private const int _detectorIdMatchingCount    = 9915;
    private const int _detectorIdNormalizedCase   = 9916;
    private const int _detectorIdNormalizedForm   = 9917;

    private const string _answerFirst      = "answer-one";
    private const string _answerSecond     = "answer-two";
    private const string _answerWithPunct  = "Is it?";
    private const string _optionA          = "Option A";
    private const string _optionB          = "Option B";
    private const string _optionAbsent     = "Option C";
    private const string _matchOptionLeft  = "Match A";
    private const string _matchOptionRight = "Match B";
    private const string _altlessImageFile = "diagram.png";
    private const string _categoryTracing  = "sledzenie_kodu";
    private const string _categoryAbsent   = "invalid_category_xyz";
    private const string _algorithmBubble  = "sortowanie_babelkowe";
    private const string _unknownTypeValue = "unknownType";
    private const string _trueToken        = "true";
    private const string _falseToken       = "false";
    private const string _polishYes        = "Tak";
    private const string _polishNo         = "Nie";
    private const string _polishYesPadded  = "  TAK ";
    private const string _polishAccented   = "Zamknięcie";

    #region Production bank assertions

    [Fact]
    public void ProductionBank_HasNoShortAnswerViolations_WhenAllQuestionsChecked()
    {
        // Arrange
        List<Question> questions = LoadProductionBank();

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
        List<Question> questions = LoadProductionBank();

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
        List<Question> questions = LoadProductionBank();
        string imagesDir = Path.Combine(FixturePath.RepoRoot(), "data", "images");

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
        string rawJson = File.ReadAllText(ProductionBankPath());

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
        List<Question> questions = LoadProductionBank();

        // Act
        List<string> violations = CollectAnswerOptionViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) where correctAnswer is not among options after normalisation:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoAnswerShapeViolations_WhenPositionalTypesChecked()
    {
        // Arrange
        List<Question> questions = LoadProductionBank();

        // Act
        List<string> violations = CollectAnswerShapeViolations(questions);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) whose correctAnswer shape contradicts ADR-024:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void ProductionBank_HasNoCategoryViolations_WhenAllQuestionsChecked()
    {
        // Arrange
        List<Question> questions = LoadProductionBank();

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
        List<Question> questions = LoadProductionBank();

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
        List<Question> questions = LoadProductionBank();
        TagVocabulary vocabulary = ParseTagVocabulary(TagsDocumentPath());

        // Act
        List<string> violations = CollectTagViolations(questions, vocabulary);

        // Assert
        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} question(s) with tag values not in docs/tags.md:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    #endregion

    #region Tag vocabulary parsing

    [Fact]
    public void ParseTagVocabulary_KeepsTheTwoAxesDisjoint_WhenRealTagsDocumentIsParsed()
    {
        // Arrange
        string tagsPath = TagsDocumentPath();

        // Act
        TagVocabulary vocabulary = ParseTagVocabulary(tagsPath);

        // Assert
        Assert.Contains(_categoryTracing, vocabulary.Categories);
        Assert.DoesNotContain(_categoryTracing, vocabulary.Algorithms);
        Assert.Contains(_algorithmBubble, vocabulary.Algorithms);
        Assert.DoesNotContain(_algorithmBubble, vocabulary.Categories);
    }

    [Fact]
    public void ParseTagVocabulary_ExcludesSourceFormatSegments_WhenRealTagsDocumentIsParsed()
    {
        // Arrange
        string[] sourceFormatSegments = ["OLYMPIAD", "YEAR", "STAGE", "PART"];
        string tagsPath = TagsDocumentPath();

        // Act
        TagVocabulary vocabulary = ParseTagVocabulary(tagsPath);

        // Assert
        foreach (string segment in sourceFormatSegments)
        {
            Assert.DoesNotContain(segment, vocabulary.Categories);
            Assert.DoesNotContain(segment, vocabulary.Algorithms);
        }
    }

    #endregion

    #region Detector assertions

    [Fact]
    public void ShortAnswerDetector_ReportsViolation_WhenCorrectAnswerHasTwoItems()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdShortAnswerTwo)
            .WithType(QuestionType.ShortAnswer)
            .WithCorrectAnswer(_answerFirst, _answerSecond)
            .Build();

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
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdShortAnswerPunct)
            .WithType(QuestionType.ShortAnswer)
            .WithCorrectAnswer(_answerWithPunct)
            .Build();

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
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdHangul)
            .WithContent(new ContentBlock { Type = ContentBlockType.Text, Text = koreanSyllables })
            .Build();

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
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdMissingImage)
            .WithContent(new ContentBlock { Type = ContentBlockType.Image, File = missingFile })
            .Build();

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
        string badJson =
            $"[{{\"id\": {_detectorIdUnknownType}, \"type\": \"{_unknownTypeValue}\", \"content\": [], " +
            "\"category\": [], \"olympiad\": \"OIJ\", \"stage\": \"E1\", \"correctAnswer\": \"x\"}]";

        // Act
        List<string> violations = CollectTypeVocabularyViolations(badJson);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdUnknownType.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerOptionsChecker_ReportsViolation_WhenCorrectAnswerIsNotInOptions()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdAnswerNotInOpts)
            .WithType(QuestionType.Single)
            .WithOptions(_optionA, _optionB)
            .WithCorrectAnswer(_optionAbsent)
            .Build();

        // Act
        List<string> violations = CollectAnswerOptionViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdAnswerNotInOpts.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerOptionsChecker_NamesTheOptionsItComparedAgainst_WhenAnswerIsNotFound()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdAnswerNotInOpts)
            .WithType(QuestionType.Single)
            .WithOptions(_optionA, _optionB)
            .WithCorrectAnswer(_optionAbsent)
            .Build();

        // Act
        string message = string.Join(" ", CollectAnswerOptionViolations([bad]));

        // Assert
        Assert.Contains(_optionA, message);
        Assert.Contains(_optionB, message);
    }

    [Fact]
    public void AnswerOptionsChecker_ReportsViolation_WhenMatchingAnswerIsNotInMatchOptions()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdMatchingAnswer)
            .WithType(QuestionType.Matching)
            .WithMatchOptions(_matchOptionLeft, _matchOptionRight)
            .WithCorrectAnswer(_matchOptionLeft, _optionAbsent)
            .Build();

        // Act
        List<string> violations = CollectAnswerOptionViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdMatchingAnswer.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerOptionsChecker_ReportsNoViolation_WhenAnswerDiffersOnlyByCaseAndPadding()
    {
        // Arrange
        Question padded = QuestionBuilder.AQuestion()
            .WithId(_detectorIdNormalizedCase)
            .WithType(QuestionType.Single)
            .WithOptions(_polishYes, _polishNo)
            .WithCorrectAnswer(_polishYesPadded)
            .Build();

        // Act
        List<string> violations = CollectAnswerOptionViolations([padded]);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public void AnswerOptionsChecker_ReportsNoViolation_WhenAnswerIsDecomposedAndOptionIsComposed()
    {
        // Arrange
        string composedOption   = _polishAccented.Normalize(NormalizationForm.FormC);
        string decomposedAnswer = _polishAccented.Normalize(NormalizationForm.FormD);
        Question decomposed = QuestionBuilder.AQuestion()
            .WithId(_detectorIdNormalizedForm)
            .WithType(QuestionType.Single)
            .WithOptions(composedOption)
            .WithCorrectAnswer(decomposedAnswer)
            .Build();

        // Act
        List<string> violations = CollectAnswerOptionViolations([decomposed]);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public void AnswerShapeChecker_ReportsViolation_WhenTrueFalseAnswerCountDiffersFromOptions()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdTrueFalseCount)
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_optionA, _optionB)
            .WithCorrectAnswer(_trueToken)
            .Build();

        // Act
        List<string> violations = CollectAnswerShapeViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdTrueFalseCount.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerShapeChecker_ReportsViolation_WhenTrueFalseAnswerCarriesOptionText()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdTrueFalseToken)
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_optionA, _optionB)
            .WithCorrectAnswer(_trueToken, _optionB)
            .Build();

        // Act
        List<string> violations = CollectAnswerShapeViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdTrueFalseToken.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerShapeChecker_ReportsViolation_WhenOrderingAnswerRepeatsAnOption()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdOrderingDupe)
            .WithType(QuestionType.Ordering)
            .WithOptions(_optionA, _optionB)
            .WithCorrectAnswer(_optionA, _optionA)
            .Build();

        // Act
        List<string> violations = CollectAnswerShapeViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdOrderingDupe.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void AnswerShapeChecker_ReportsViolation_WhenMatchingAnswerCountDiffersFromOptions()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdMatchingCount)
            .WithType(QuestionType.Matching)
            .WithOptions(_optionA, _optionB)
            .WithMatchOptions(_matchOptionLeft, _matchOptionRight)
            .WithCorrectAnswer(_matchOptionLeft)
            .Build();

        // Act
        List<string> violations = CollectAnswerShapeViolations([bad]);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdMatchingCount.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void CategoryChecker_ReportsViolation_WhenCategoryIsEmpty()
    {
        // Arrange
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdEmptyCategory)
            .WithoutCategory()
            .Build();

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
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdMissingAlt)
            .WithContent(new ContentBlock { Type = ContentBlockType.Image, File = _altlessImageFile, Alt = null })
            .Build();

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
        TagVocabulary vocabulary = new([_categoryTracing], [_algorithmBubble]);
        Question bad = QuestionBuilder.AQuestion()
            .WithId(_detectorIdUnknownTag)
            .WithCategory(_categoryTracing, _categoryAbsent)
            .Build();

        // Act
        List<string> violations = CollectTagViolations([bad], vocabulary);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(_detectorIdUnknownTag.ToString(), string.Join(" ", violations));
    }

    [Fact]
    public void TagChecker_ReportsViolation_WhenAnAlgorithmTagIsUsedAsACategory()
    {
        // Arrange
        TagVocabulary vocabulary = new([_categoryTracing], [_algorithmBubble]);
        Question crossAxis = QuestionBuilder.AQuestion()
            .WithId(_detectorIdCrossAxisTag)
            .WithCategory(_algorithmBubble)
            .WithAlgorithms(_categoryTracing)
            .Build();

        // Act
        List<string> violations = CollectTagViolations([crossAxis], vocabulary);

        // Assert
        Assert.Equal(2, violations.Count);
        Assert.Contains(_detectorIdCrossAxisTag.ToString(), string.Join(" ", violations));
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
                violations.Add($"  id={question.Id}: correctAnswer ends with '?' or '.' — \"{EscapeInvisible(answer)}\"");
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
            foreach (ContentBlock block in GatherBlocks(question))
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
                    // Both sides are printed, escaped: a mismatch between two strings that render
                    // identically is exactly the failure this message has to make readable.
                    string displayAnswer = EscapeInvisible(answer);
                    string displayPool = string.Join(", ", optionPool.Select(option => $"\"{EscapeInvisible(option)}\""));
                    violations.Add(
                        $"  id={question.Id} ({question.Type}): correctAnswer \"{displayAnswer}\" " +
                        $"not found in options after normalisation. Options: {displayPool}");
                }
            }
        }

        return violations;
    }

    private static List<string> CollectAnswerShapeViolations(List<Question> questions)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            List<string> answers = question.CorrectAnswer ?? [];
            List<string> options = question.Options ?? [];

            switch (question.Type)
            {
                // ADR-024: one "true"/"false" entry per options entry, positional.
                case QuestionType.TrueFalse:
                    if (answers.Count != options.Count)
                    {
                        violations.Add(
                            $"  id={question.Id} (TrueFalse): expected {options.Count} answer(s) to match options, found {answers.Count}");
                    }

                    foreach (string answer in answers)
                    {
                        if (!_trueFalseTokens.Contains(answer ?? string.Empty))
                        {
                            violations.Add(
                                $"  id={question.Id} (TrueFalse): correctAnswer \"{EscapeInvisible(answer)}\" is not \"{_trueToken}\" or \"{_falseToken}\"");
                        }
                    }

                    break;

                // ADR-024: option values in correct order — so a permutation, never a repeat.
                case QuestionType.Ordering:
                    if (answers.Count != options.Count)
                    {
                        violations.Add(
                            $"  id={question.Id} (Ordering): expected {options.Count} answer(s) to match options, found {answers.Count}");
                    }

                    if (answers.Distinct(StringComparer.Ordinal).Count() != answers.Count)
                    {
                        violations.Add(
                            $"  id={question.Id} (Ordering): correctAnswer repeats a value, so it is not a permutation of options");
                    }

                    break;

                // ADR-024: matchOptions values, positional, aligned to options by index.
                case QuestionType.Matching:
                    if (answers.Count != options.Count)
                    {
                        violations.Add(
                            $"  id={question.Id} (Matching): expected {options.Count} answer(s) aligned to options by index, found {answers.Count}");
                    }

                    break;
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
            foreach (ContentBlock block in GatherBlocks(question))
            {
                if (block.Type == ContentBlockType.Image && string.IsNullOrEmpty(block.Alt))
                {
                    string fileName = block.File ?? "(none)";
                    violations.Add($"  id={question.Id}: image block (file={fileName}) has no alt text");
                }
            }
        }

        return violations;
    }

    private static List<string> CollectTagViolations(List<Question> questions, TagVocabulary vocabulary)
    {
        List<string> violations = [];

        foreach (Question question in questions)
        {
            foreach (string tag in question.Category ?? [])
            {
                if (!vocabulary.Categories.Contains(tag))
                {
                    violations.Add(
                        $"  id={question.Id}: category tag \"{EscapeInvisible(tag)}\" is not a category in docs/tags.md. " +
                        $"Valid: {FormatVocabulary(vocabulary.Categories)}");
                }
            }

            foreach (string tag in question.Algorithms ?? [])
            {
                if (!vocabulary.Algorithms.Contains(tag))
                {
                    violations.Add(
                        $"  id={question.Id}: algorithms tag \"{EscapeInvisible(tag)}\" is not an algorithm in docs/tags.md. " +
                        $"Valid: {FormatVocabulary(vocabulary.Algorithms)}");
                }
            }
        }

        return violations;
    }

    // The two axes are separate controlled vocabularies, so they parse into separate sets keyed off
    // the "##" heading. Merging them would let an algorithm value pass as a category. Section
    // tracking also keeps the "source" format table out: its rows are backticked like tag rows and
    // would otherwise contribute OLYMPIAD, YEAR, STAGE and PART as valid tag values.
    private static TagVocabulary ParseTagVocabulary(string tagsPath)
    {
        const string categoryHeading   = "`category[]`";
        const string algorithmsHeading = "`algorithms[]`";
        const string headingPrefix     = "## ";
        const string tagRowPrefix      = "| `";

        HashSet<string> categories = new(StringComparer.Ordinal);
        HashSet<string> algorithms = new(StringComparer.Ordinal);
        HashSet<string> currentAxis = null;

        foreach (string line in File.ReadAllLines(tagsPath))
        {
            if (line.StartsWith(headingPrefix, StringComparison.Ordinal))
            {
                currentAxis = line.Contains(categoryHeading, StringComparison.Ordinal) ? categories
                    : line.Contains(algorithmsHeading, StringComparison.Ordinal) ? algorithms
                    : null;
                continue;
            }

            if (currentAxis == null || !line.StartsWith(tagRowPrefix, StringComparison.Ordinal))
                continue;

            int closeBacktick = line.IndexOf('`', tagRowPrefix.Length);
            if (closeBacktick < 0)
                continue;

            string tag = line[tagRowPrefix.Length..closeBacktick];
            if (tag.Length > 0)
                currentAxis.Add(tag);
        }

        return new TagVocabulary(categories, algorithms);
    }

    #endregion

    #region Helpers

    private sealed record TagVocabulary(HashSet<string> Categories, HashSet<string> Algorithms);

    private static string ProductionBankPath()
    {
        return Path.Combine(FixturePath.RepoRoot(), "data", "questions.json");
    }

    private static string TagsDocumentPath()
    {
        return Path.Combine(FixturePath.RepoRoot(), "docs", "tags.md");
    }

    private static List<Question> LoadProductionBank()
    {
        string json = File.ReadAllText(ProductionBankPath());
        return JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);
    }

    private static List<ContentBlock> GatherBlocks(Question question)
    {
        List<ContentBlock> blocks = [];

        if (question.Content != null)
            blocks.AddRange(question.Content);
        if (question.ContentCpp != null)
            blocks.AddRange(question.ContentCpp);
        if (question.Explanation != null)
            blocks.AddRange(question.Explanation);

        return blocks;
    }

    private static string FormatVocabulary(HashSet<string> vocabulary)
    {
        return string.Join(", ", vocabulary.OrderBy(tag => tag, StringComparer.Ordinal));
    }

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

    // char.IsControl covers only C0/C1, which this corpus never contains. The invisibles it does
    // contain — non-breaking space, zero-width space, zero-width joiner, soft hyphen, BOM, figure
    // space — are Format or SpaceSeparator, so escaping keys off the Unicode category instead.
    private static string EscapeInvisible(string value)
    {
        if (value == null)
            return "(null)";

        StringBuilder builder = new(value.Length * 2);
        foreach (char c in value)
        {
            if (IsInvisible(c))
                builder.Append($"\\u{(int)c:X4}");
            else
                builder.Append(c);
        }

        return builder.ToString();
    }

    private static bool IsInvisible(char c)
    {
        const char ordinarySpace = ' ';

        UnicodeCategory category = char.GetUnicodeCategory(c);

        if (category == UnicodeCategory.SpaceSeparator)
            return c != ordinarySpace;

        return category is UnicodeCategory.Control
            or UnicodeCategory.Format
            or UnicodeCategory.Surrogate
            or UnicodeCategory.PrivateUse
            or UnicodeCategory.OtherNotAssigned;
    }

    #endregion
}
