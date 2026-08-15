using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L0.Sqlite;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class SqliteQuestionRepositoryTests
{
    private const string _categoryRecursion = "rekurencja";
    private const string _categoryGraphs    = "grafy";
    private const string _algorithmBfs      = "BFS";
    private const string _algorithmDfs      = "DFS";
    private const string _absentTag         = "unknownTag";
    private const string _knownStage        = "E1";
    private const string _secondStage       = "E2";
    private const string _absentStage       = "UNKNOWN_STAGE";
    private const int    _knownYear         = 2023;
    private const int    _secondYear        = 2024;
    private const int    _absentYear        = 1999;

    private const int _poolLargerThanMaxLimit = 40;
    private const int _poolSmallerThanLimit   = 10;
    private const int _limitWithinRange       = 5;
    private const int _limitAboveMaximum      = 999;
    private const int _negativeLimit          = -5;
    private const int _generousLimit          = 10;
    private const int _shufflePoolSize        = 5;
    private const int _limitBelowPoolSize     = 3;

    private const int _distinctStageCount     = 2;
    private const int _distinctYearCount      = 2;
    private const int _distinctCategoryCount  = 2;
    private const int _distinctAlgorithmCount = 2;

    // id=1 -> category=[rekurencja],         algorithms=[BFS]
    // id=2 -> category=[grafy],              algorithms=[DFS]
    // id=3 -> category=[rekurencja, grafy],  algorithms=[BFS, DFS]
    private static readonly QuestionCandidate[] _threeCandidates =
    [
        new QuestionCandidate { Id = 1, Category = "[\"rekurencja\"]",             Algorithms = "[\"BFS\"]" },
        new QuestionCandidate { Id = 2, Category = "[\"grafy\"]",                  Algorithms = "[\"DFS\"]" },
        new QuestionCandidate { Id = 3, Category = "[\"rekurencja\",\"grafy\"]",   Algorithms = "[\"BFS\",\"DFS\"]" }
    ];

    #region Constructor

    [Fact]
    public void Constructor_CallsLoadSummaryExactlyOnce_WhenCreated()
    {
        // Arrange
        Mock<IQuestionStore> store = new(MockBehavior.Strict);
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());

        // Act
        Build(store);

        // Assert
        store.Verify(s => s.LoadSummary(), Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotCallSelectCandidatesOrFetchByIds_WhenCreated()
    {
        // Arrange
        Mock<IQuestionStore> store = new(MockBehavior.Strict);
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());

        // Act
        Build(store);

        // Assert
        store.Verify(s => s.SelectCandidates(
            It.IsAny<IReadOnlyCollection<string>>(),
            It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
        store.Verify(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()), Times.Never);
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task GetAsync_ThrowsOperationCanceledException_WhenTokenAlreadyCancelled()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.GetAsync(new QuestionQuery(), cts.Token));
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ThrowsOperationCanceledException_WhenTokenAlreadyCancelled()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.GetFilterOptionsAsync(cts.Token));
    }

    #endregion

    #region Limit clamping

    [Fact]
    public async Task GetAsync_ClampsToDefaultLimit_WhenLimitIsZero()
    {
        // Arrange
        const int zeroLimit = 0;
        Mock<IQuestionStore> store = StoreReturningCandidates(UntaggedCandidates(_poolLargerThanMaxLimit));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = zeroLimit }, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.DefaultLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_ClampsToDefaultLimit_WhenLimitIsNegative()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(UntaggedCandidates(_poolLargerThanMaxLimit));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _negativeLimit }, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.DefaultLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_ClampsToMaxLimit_WhenLimitExceedsMaximum()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(UntaggedCandidates(_poolLargerThanMaxLimit));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _limitAboveMaximum }, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.MaxLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_PreservesLimit_WhenLimitIsWithinRange()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(UntaggedCandidates(_poolSmallerThanLimit));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _limitWithinRange }, CancellationToken.None);

        // Assert
        Assert.Equal(_limitWithinRange, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllCandidates_WhenCountIsBelowLimit()
    {
        // Arrange
        QuestionCandidate[] twoCandidates = [_threeCandidates[0], _threeCandidates[1]];
        Mock<IQuestionStore> store = StoreReturningCandidates(twoCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _generousLimit }, CancellationToken.None);

        // Assert
        Assert.Equal(twoCandidates.Length, result.Count);
    }

    #endregion

    #region Tag matching — OR within a type, AND across types

    [Fact]
    public async Task GetAsync_ReturnsAllCandidates_WhenNoTagFiltersAreSet()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _generousLimit }, CancellationToken.None);

        // Assert
        Assert.Equal(_threeCandidates.Length, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyTaggedQuestions_WhenOneCategoryIsRequested()
    {
        // Arrange
        int[] expectedIds = [1, 3];
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        QuestionQuery query = new() { Limit = _generousLimit, Categories = [_categoryRecursion] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedIds, result.Select(q => q.Id).Order().ToArray());
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyTaggedQuestions_WhenOneAlgorithmIsRequested()
    {
        // Arrange
        int[] expectedIds = [2, 3];
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        QuestionQuery query = new() { Limit = _generousLimit, Algorithms = [_algorithmDfs] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedIds, result.Select(q => q.Id).Order().ToArray());
    }

    [Fact]
    public async Task GetAsync_AppliesAndAcrossTypes_WhenBothCategoryAndAlgorithmAreSet()
    {
        // Arrange
        int[] expectedIds = [3];
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        QuestionQuery query = new()
        {
            Limit      = _generousLimit,
            Categories = [_categoryRecursion],
            Algorithms = [_algorithmDfs]
        };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedIds, result.Select(q => q.Id).Order().ToArray());
    }

    [Fact]
    public async Task GetAsync_ReturnsQuestionsMatchingEitherTag_WhenTwoCategoriesAreRequested()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        QuestionQuery query = new()
        {
            Limit      = _generousLimit,
            Categories = [_categoryRecursion, _categoryGraphs]
        };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(_threeCandidates.Length, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsEmpty_WhenTagFilterMatchesNothing()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);
        QuestionQuery query = new() { Limit = _generousLimit, Categories = [_absentTag] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Shuffle then cap

    [Fact]
    public async Task GetAsync_CallsShufflerOnce_WhenCandidatesAreMatched()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        Mock<IShuffler> shuffler = new();
        (SqliteQuestionRepository repo, _) = Build(store, shuffler);

        // Act
        await repo.GetAsync(new QuestionQuery { Limit = _generousLimit }, CancellationToken.None);

        // Assert
        shuffler.Verify(s => s.Shuffle(It.IsAny<IList<QuestionCandidate>>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_PreservesShufflerOrder_WhenShufflerReordersCandidates()
    {
        // Arrange
        int[] expectedIds = [3, 2, 1];
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store, ReversingShuffler());

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _generousLimit }, CancellationToken.None);

        // Assert
        Assert.Equal(expectedIds, result.Select(q => q.Id).ToArray());
    }

    [Fact]
    public async Task GetAsync_CapsAfterShuffling_WhenCandidateCountExceedsLimit()
    {
        // Arrange — reversing five candidates gives [5,4,3,2,1]; capping after the shuffle
        // takes the first three of that, whereas capping first would yield [1,2,3].
        int[] expectedIds = [5, 4, 3];
        Mock<IQuestionStore> store = StoreReturningCandidates(UntaggedCandidates(_shufflePoolSize));
        (SqliteQuestionRepository repo, _) = Build(store, ReversingShuffler());

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _limitBelowPoolSize }, CancellationToken.None);

        // Assert
        Assert.Equal(expectedIds, result.Select(q => q.Id).ToArray());
    }

    #endregion

    #region Seam faults bubble rather than being swallowed

    [Fact]
    public async Task GetAsync_Bubbles_WhenSelectCandidatesThrowsSqliteException()
    {
        // Arrange
        const string sqliteMessage = "database is locked";
        const int sqliteBusyErrorCode = 5;
        Mock<IQuestionStore> store = StoreThrowingFromSelectCandidates(
            new SqliteException(sqliteMessage, sqliteBusyErrorCode));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(
            () => repo.GetAsync(new QuestionQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Bubbles_WhenSelectCandidatesThrowsIOException()
    {
        // Arrange
        const string diskMessage = "disk error";
        Mock<IQuestionStore> store = StoreThrowingFromSelectCandidates(new IOException(diskMessage));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            () => repo.GetAsync(new QuestionQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Bubbles_WhenSelectCandidatesThrowsUnanticipatedException()
    {
        // Arrange — nothing in the seam's contract predicts this; it must still reach the caller,
        // because the global middleware is the only thing entitled to shape an unknown fault.
        const string unanticipatedMessage = "nobody predicted this";
        Mock<IQuestionStore> store = StoreThrowingFromSelectCandidates(
            new NotSupportedException(unanticipatedMessage));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => repo.GetAsync(new QuestionQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Bubbles_WhenFetchByIdsThrowsInvalidOperationException()
    {
        // Arrange
        const string stateMessage = "unexpected state";
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Throws(new InvalidOperationException(stateMessage));
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.GetAsync(new QuestionQuery { Limit = _generousLimit }, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Bubbles_WhenRowCarriesMalformedJson()
    {
        // Arrange — the row survives the seam, so the JSON fault is raised above it, in mapping.
        const string malformedJson = "{ this is not json";
        QuestionRow malformedRow = MinimalRow(1);
        malformedRow.Content = malformedJson;
        Mock<IQuestionStore> store = StoreReturningCandidates([_threeCandidates[0]]);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>())).Returns([malformedRow]);
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(
            () => repo.GetAsync(new QuestionQuery { Limit = _generousLimit }, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Bubbles_WhenShufflerThrows()
    {
        // Arrange
        const string shufflerMessage = "shuffler failed";
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        Mock<IShuffler> shuffler = new();
        shuffler.Setup(s => s.Shuffle(It.IsAny<IList<QuestionCandidate>>()))
            .Throws(new InvalidOperationException(shufflerMessage));
        (SqliteQuestionRepository repo, _) = Build(store, shuffler);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.GetAsync(new QuestionQuery { Limit = _generousLimit }, CancellationToken.None));
    }

    [Fact]
    public void Constructor_Bubbles_WhenLoadSummaryThrowsInvalidOperationException()
    {
        // Arrange
        const string schemaMessage = "schema mismatch";
        Mock<IQuestionStore> store = new();
        store.Setup(s => s.LoadSummary()).Throws(new InvalidOperationException(schemaMessage));
        Mock<IShuffler> shuffler = new();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => new SqliteQuestionRepository(
                store.Object,
                shuffler.Object,
                new CapturingLogger<SqliteQuestionRepository>()));
    }

    [Fact]
    public async Task GetAsync_ThrowsInvalidOperationException_WhenRowHasUnrecognisedTypeString()
    {
        // Arrange
        const string unrecognisedType = "bogus";
        Mock<IQuestionStore> store = StoreReturningCandidates([_threeCandidates[0]]);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Returns([MinimalRow(1, unrecognisedType)]);
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.GetAsync(new QuestionQuery { Limit = _generousLimit }, CancellationToken.None));
    }

    #endregion

    #region Unknown-value warnings

    [Fact]
    public async Task GetAsync_LogsWarning_WhenStageIsNotInKnownSet()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates([]);
        (SqliteQuestionRepository repo, CapturingLogger<SqliteQuestionRepository> logger) = Build(store);
        QuestionQuery query = new() { Stages = [_absentStage] };

        // Act
        await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Contains(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains(_absentStage));
    }

    [Fact]
    public async Task GetAsync_LogsWarning_WhenYearIsNotInKnownSet()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates([]);
        (SqliteQuestionRepository repo, CapturingLogger<SqliteQuestionRepository> logger) = Build(store);
        QuestionQuery query = new() { Years = [_absentYear] };

        // Act
        await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Contains(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains(_absentYear.ToString()));
    }

    [Fact]
    public async Task GetAsync_DoesNotLogWarning_WhenAllValuesAreKnown()
    {
        // Arrange
        const string unknownValueMarker = "not present";
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, CapturingLogger<SqliteQuestionRepository> logger) = Build(store);
        QuestionQuery query = new() { Stages = [_knownStage], Years = [_knownYear] };

        // Act
        await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains(unknownValueMarker));
    }

    #endregion

    #region Row to Question mapping

    [Fact]
    public async Task GetAsync_MapsEveryRowField_WhenRowIsFullyPopulated()
    {
        // Arrange
        const int    mappedId          = 42;
        const string mappedOlympiad    = "OIJ";
        const int    mappedYear        = 2025;
        const int    mappedDifficulty  = 3;
        const string mappedSource      = "Olimpiada 2025";
        const string mappedSourceUrl   = "https://example.com/task";
        const string mappedSourceRaw   = "Source raw text";
        const string mappedExplanation = "Explanation source";
        const int    mappedPoints      = 2;
        const int    partialCreditOn   = 1;
        string[] mappedOptions = ["A", "B", "C"];
        string[] mappedAnswers = ["A", "C"];

        QuestionRow fullRow = new()
        {
            Id                = mappedId,
            Olympiad          = mappedOlympiad,
            Stage             = _secondStage,
            Year              = mappedYear,
            Difficulty        = mappedDifficulty,
            Source            = mappedSource,
            SourceUrl         = mappedSourceUrl,
            SourceRaw         = mappedSourceRaw,
            ExplanationSource = mappedExplanation,
            Type              = "multi",
            Content           = "[{\"Type\":\"text\",\"Text\":\"Which?\"}]",
            Options           = "[\"A\",\"B\",\"C\"]",
            CorrectAnswer     = "[\"A\",\"C\"]",
            Category          = "[\"grafy\"]",
            Algorithms        = "[\"DFS\"]",
            Points            = mappedPoints,
            PartialCredit     = partialCreditOn
        };

        Mock<IQuestionStore> store = StoreReturningCandidates(
            [new QuestionCandidate { Id = mappedId, Category = "[\"grafy\"]", Algorithms = "[\"DFS\"]" }]);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>())).Returns([fullRow]);
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(
            new QuestionQuery { Limit = _generousLimit }, CancellationToken.None);

        // Assert
        Question mapped = Assert.Single(result);
        Assert.Equal(mappedId, mapped.Id);
        Assert.Equal(mappedOlympiad, mapped.Olympiad);
        Assert.Equal(_secondStage, mapped.Stage);
        Assert.Equal(mappedYear, mapped.Year);
        Assert.Equal(mappedDifficulty, mapped.Difficulty);
        Assert.Equal(mappedSource, mapped.Source);
        Assert.Equal(mappedSourceUrl, mapped.SourceUrl);
        Assert.Equal(mappedSourceRaw, mapped.SourceRaw);
        Assert.Equal(mappedExplanation, mapped.ExplanationSource);
        Assert.Equal(QuestionType.Multi, mapped.Type);
        Assert.Equal(mappedOptions, mapped.Options);
        Assert.Equal(mappedAnswers, mapped.CorrectAnswer);
        Assert.Equal([_categoryGraphs], mapped.Category);
        Assert.Equal([_algorithmDfs], mapped.Algorithms);
        Assert.Equal(mappedPoints, mapped.Points);
        Assert.True(mapped.PartialCredit);
    }

    #endregion

    #region Cached filter options

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsCachedOptions_WhenCalledAfterConstruction()
    {
        // Arrange — a strict mock throws on any store call the constructor did not already make.
        Mock<IQuestionStore> store = new(MockBehavior.Strict);
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        FilterOptions options = await repo.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(_threeCandidates.Length, options.TotalQuestions);
        Assert.Equal(_distinctStageCount, options.Stages.Count);
        Assert.Equal(_distinctYearCount, options.Years.Count);
        Assert.Equal(_distinctCategoryCount, options.Categories.Count);
        Assert.Equal(_distinctAlgorithmCount, options.Algorithms.Count);
        store.Verify(s => s.LoadSummary(), Times.Once);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsSameInstance_WhenCalledTwice()
    {
        // Arrange
        Mock<IQuestionStore> store = StoreReturningCandidates(_threeCandidates);
        (SqliteQuestionRepository repo, _) = Build(store);

        // Act
        FilterOptions first  = await repo.GetFilterOptionsAsync(CancellationToken.None);
        FilterOptions second = await repo.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Same(first, second);
    }

    #endregion

    #region Helpers

    private static BankSummary DefaultSummary()
    {
        return new BankSummary
        {
            TotalCount = _threeCandidates.Length,
            Stages =
            [
                new FilterOption { Value = _knownStage,  Count = 2 },
                new FilterOption { Value = _secondStage, Count = 1 }
            ],
            Years =
            [
                new FilterOption { Value = _knownYear.ToString(),  Count = 2 },
                new FilterOption { Value = _secondYear.ToString(), Count = 1 }
            ],
            CategoryJsons  = ["[\"rekurencja\"]", "[\"grafy\"]", "[\"rekurencja\",\"grafy\"]"],
            AlgorithmJsons = ["[\"BFS\"]",        "[\"DFS\"]",   "[\"BFS\",\"DFS\"]"]
        };
    }

    private static QuestionRow MinimalRow(int id, string type = "single")
    {
        return new QuestionRow
        {
            Id            = id,
            Olympiad      = "OIJ",
            Stage         = _knownStage,
            Year          = _knownYear,
            Type          = type,
            Content       = "[{\"Type\":\"text\",\"Text\":\"Q?\"}]",
            Options       = "[\"A\",\"B\"]",
            CorrectAnswer = "[\"A\"]",
            Category      = "[\"rekurencja\"]",
            Algorithms    = "[\"BFS\"]",
            Points        = 1,
            PartialCredit = 0
        };
    }

    private static List<QuestionCandidate> UntaggedCandidates(int count)
    {
        return [.. Enumerable.Range(1, count)
            .Select(id => new QuestionCandidate { Id = id, Category = "[]", Algorithms = "[]" })];
    }

    private static (SqliteQuestionRepository Repo, CapturingLogger<SqliteQuestionRepository> Logger)
        Build(Mock<IQuestionStore> store, Mock<IShuffler> shuffler = null)
    {
        CapturingLogger<SqliteQuestionRepository> logger = new();
        shuffler ??= new Mock<IShuffler>();
        return (new SqliteQuestionRepository(store.Object, shuffler.Object, logger), logger);
    }

    private static Mock<IQuestionStore> StoreReturningCandidates(
        IReadOnlyList<QuestionCandidate> candidates, BankSummary summary = null)
    {
        Mock<IQuestionStore> store = new(MockBehavior.Loose);
        store.Setup(s => s.LoadSummary()).Returns(summary ?? DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Returns(candidates);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Returns((IReadOnlyList<int> ids) => ids.Select(id => MinimalRow(id)).ToList());
        return store;
    }

    private static Mock<IQuestionStore> StoreThrowingFromSelectCandidates(Exception fault)
    {
        Mock<IQuestionStore> store = new();
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Throws(fault);
        return store;
    }

    private static Mock<IShuffler> ReversingShuffler()
    {
        Mock<IShuffler> shuffler = new();
        shuffler.Setup(s => s.Shuffle(It.IsAny<IList<QuestionCandidate>>()))
            .Callback<IList<QuestionCandidate>>(list =>
            {
                int count = list.Count;
                for (int i = 0; i < count / 2; i++)
                {
                    (list[i], list[count - 1 - i]) = (list[count - 1 - i], list[i]);
                }
            });
        return shuffler;
    }

    #endregion
}
