using Microsoft.Extensions.Logging;
using Moq;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L0.Sqlite;

/// <summary>
/// L0 tests for SqliteQuestionRepository — all I/O mocked via IQuestionStore.
/// Covers: limit clamping, tag matching, shuffle ordering, seam-exception bubbling,
/// unknown-value warnings, cached filter options, and cancellation.
/// </summary>
[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class SqliteQuestionRepositoryTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    // Three base candidates used across most tests.
    // id=1 → category=["rekurencja"],         algorithms=["BFS"]
    // id=2 → category=["grafy"],              algorithms=["DFS"]
    // id=3 → category=["rekurencja","grafy"], algorithms=["BFS","DFS"]
    private static readonly QuestionCandidate[] _threeCandidates =
    [
        new QuestionCandidate { Id = 1, Category = "[\"rekurencja\"]",        Algorithms = "[\"BFS\"]"       },
        new QuestionCandidate { Id = 2, Category = "[\"grafy\"]",             Algorithms = "[\"DFS\"]"       },
        new QuestionCandidate { Id = 3, Category = "[\"rekurencja\",\"grafy\"]", Algorithms = "[\"BFS\",\"DFS\"]" }
    ];

    private static BankSummary DefaultSummary() => new()
    {
        TotalCount     = 3,
        Stages         = [new FilterOption { Value = "E1", Count = 2 }, new FilterOption { Value = "E2", Count = 1 }],
        Years          = [new FilterOption { Value = "2023", Count = 2 }, new FilterOption { Value = "2024", Count = 1 }],
        CategoryJsons  = ["[\"rekurencja\"]", "[\"grafy\"]", "[\"rekurencja\",\"grafy\"]"],
        AlgorithmJsons = ["[\"BFS\"]",        "[\"DFS\"]",   "[\"BFS\",\"DFS\"]"]
    };

    private static QuestionRow MinimalRow(int id, string type = "single") => new()
    {
        Id           = id,
        Olympiad     = "OIJ",
        Stage        = "E1",
        Year         = 2023,
        Type         = type,
        Content      = "[{\"Type\":\"text\",\"Text\":\"Q?\"}]",
        Options      = "[\"A\",\"B\"]",
        CorrectAnswer = "[\"A\"]",
        Category     = "[\"rekurencja\"]",
        Algorithms   = "[\"BFS\"]",
        Points       = 1,
        PartialCredit = 0
    };

    /// <summary>Builds a repository with the given mocks and a no-op shuffler unless provided.</summary>
    private static (SqliteQuestionRepository Repo, CapturingLogger<SqliteQuestionRepository> Logger)
        Build(Mock<IQuestionStore> store, Mock<IShuffler> shuffler = null)
    {
        var logger = new CapturingLogger<SqliteQuestionRepository>();
        shuffler ??= new Mock<IShuffler>();
        return (new SqliteQuestionRepository(store.Object, shuffler.Object, logger), logger);
    }

    private static Mock<IQuestionStore> StoreReturningCandidates(
        IReadOnlyList<QuestionCandidate> candidates, BankSummary summary = null)
    {
        var store = new Mock<IQuestionStore>(MockBehavior.Loose);
        store.Setup(s => s.LoadSummary()).Returns(summary ?? DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Returns(candidates);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Returns((IReadOnlyList<int> ids) =>
                ids.Select(id => MinimalRow(id)).ToList());
        return store;
    }

    // -------------------------------------------------------------------------
    // Constructor: LoadSummary called once at construction
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_CallsLoadSummaryExactlyOnce_WhenCreated()
    {
        // Arrange
        var store = new Mock<IQuestionStore>(MockBehavior.Strict);
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());

        // Act
        Build(store);

        // Assert — Strict mock would throw if LoadSummary called more than set-up times
        store.Verify(s => s.LoadSummary(), Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotCallSelectCandidatesOrFetchByIds_WhenCreated()
    {
        // Arrange
        var store = new Mock<IQuestionStore>(MockBehavior.Strict);
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());

        // Act
        Build(store);

        // Assert — Strict mock: only LoadSummary should have been called
        store.Verify(s => s.SelectCandidates(
            It.IsAny<IReadOnlyCollection<string>>(),
            It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
        store.Verify(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // GetAsync — Cancellation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_ThrowsOperationCanceledException_WhenTokenAlreadyCancelled()
    {
        // Arrange
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.GetAsync(new QuestionQuery(), cts.Token));
    }

    // -------------------------------------------------------------------------
    // GetAsync — Limit clamping (ADR-025)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_ClampsToDefaultLimit_WhenLimitIsZero()
    {
        // Arrange — 40 candidates, limit=0 should yield DefaultLimit=30
        List<QuestionCandidate> candidates = Enumerable.Range(1, 40)
            .Select(i => new QuestionCandidate { Id = i, Category = "[]", Algorithms = "[]" })
            .ToList();
        var store = StoreReturningCandidates(candidates);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 0 }, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.DefaultLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_ClampsToDefaultLimit_WhenLimitIsNegative()
    {
        // Arrange — 40 candidates, limit=-5 should yield DefaultLimit=30
        List<QuestionCandidate> candidates = Enumerable.Range(1, 40)
            .Select(i => new QuestionCandidate { Id = i, Category = "[]", Algorithms = "[]" })
            .ToList();
        var store = StoreReturningCandidates(candidates);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = -5 }, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.DefaultLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_ClampsToMaxLimit_WhenLimitExceedsMaximum()
    {
        // Arrange — 40 candidates, limit=999 should yield MaxLimit=30
        List<QuestionCandidate> candidates = Enumerable.Range(1, 40)
            .Select(i => new QuestionCandidate { Id = i, Category = "[]", Algorithms = "[]" })
            .ToList();
        var store = StoreReturningCandidates(candidates);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 999 }, CancellationToken.None);

        // Assert
        Assert.Equal(QuestionQuery.MaxLimit, result.Count);
    }

    [Fact]
    public async Task GetAsync_PreservesLimit_WhenLimitIsWithinRange()
    {
        // Arrange — 10 candidates, limit=5 should return exactly 5
        List<QuestionCandidate> candidates = Enumerable.Range(1, 10)
            .Select(i => new QuestionCandidate { Id = i, Category = "[]", Algorithms = "[]" })
            .ToList();
        var store = StoreReturningCandidates(candidates);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 5 }, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllCandidates_WhenCountIsBelowLimit()
    {
        // Arrange — only 2 candidates, limit=10
        var store = StoreReturningCandidates([_threeCandidates[0], _threeCandidates[1]]);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // -------------------------------------------------------------------------
    // GetAsync — Tag matching (OR within type, AND across types)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_ReturnsAllCandidates_WhenNoTagFiltersAreSet()
    {
        // Arrange
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAsync_FiltersByCategory_UsingOrWithinType()
    {
        // Arrange — filter "rekurencja" should match id=1 (exact) and id=3 (combined)
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        var query = new QuestionQuery { Limit = 10, Categories = ["rekurencja"] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([1, 3], result.Select(q => q.Id).Order().ToArray());
    }

    [Fact]
    public async Task GetAsync_FiltersByAlgorithm_UsingOrWithinType()
    {
        // Arrange — filter "DFS" should match id=2 and id=3
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        var query = new QuestionQuery { Limit = 10, Algorithms = ["DFS"] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([2, 3], result.Select(q => q.Id).Order().ToArray());
    }

    [Fact]
    public async Task GetAsync_AppliesAndAcrossTypes_WhenBothCategoryAndAlgorithmAreSet()
    {
        // Arrange — "rekurencja" AND "DFS" → only id=3 has both
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        var query = new QuestionQuery { Limit = 10, Categories = ["rekurencja"], Algorithms = ["DFS"] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal([3], result.Select(q => q.Id).Order().ToArray());
    }

    [Fact]
    public async Task GetAsync_MatchesMultipleTagsWithinOneType_UsingOr()
    {
        // Arrange — "rekurencja" OR "grafy" should match all three
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        var query = new QuestionQuery { Limit = 10, Categories = ["rekurencja", "grafy"] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsEmpty_WhenTagFilterMatchesNothing()
    {
        // Arrange — "unknownTag" matches no candidate
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        var query = new QuestionQuery { Limit = 10, Categories = ["unknownTag"] };

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    // -------------------------------------------------------------------------
    // GetAsync — Shuffle called, order preserved through FetchByIds
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_CallsShuffler_WithMatchedCandidateList()
    {
        // Arrange
        var store = StoreReturningCandidates(_threeCandidates);
        var shufflerMock = new Mock<IShuffler>();
        var (repo, _) = Build(store, shufflerMock);

        // Act
        await repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None);

        // Assert
        shufflerMock.Verify(s => s.Shuffle(It.IsAny<IList<QuestionCandidate>>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_PreservesShuffledOrder_InReturnedList()
    {
        // Arrange — shuffler reverses the list so order becomes [3, 2, 1]
        var store = StoreReturningCandidates(_threeCandidates);
        var shufflerMock = new Mock<IShuffler>();
        shufflerMock.Setup(s => s.Shuffle(It.IsAny<IList<QuestionCandidate>>()))
            .Callback<IList<QuestionCandidate>>(list =>
            {
                // Reverse in place
                int n = list.Count;
                for (int i = 0; i < n / 2; i++)
                {
                    (list[i], list[n - 1 - i]) = (list[n - 1 - i], list[i]);
                }
            });

        var (repo, _) = Build(store, shufflerMock);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None);

        // Assert — result should be in reversed order: [3, 2, 1]
        Assert.Equal([3, 2, 1], result.Select(q => q.Id).ToArray());
    }

    [Fact]
    public async Task GetAsync_CapsAfterShuffle_NotBeforeShuffle()
    {
        // Arrange — 5 candidates, limit=3; shuffler reverses so ids [5,4,3,2,1] → take first 3 = [5,4,3]
        List<QuestionCandidate> candidates = Enumerable.Range(1, 5)
            .Select(i => new QuestionCandidate { Id = i, Category = "[]", Algorithms = "[]" })
            .ToList();
        var store = StoreReturningCandidates(candidates);
        var shufflerMock = new Mock<IShuffler>();
        shufflerMock.Setup(s => s.Shuffle(It.IsAny<IList<QuestionCandidate>>()))
            .Callback<IList<QuestionCandidate>>(list =>
            {
                int n = list.Count;
                for (int i = 0; i < n / 2; i++)
                {
                    (list[i], list[n - 1 - i]) = (list[n - 1 - i], list[i]);
                }
            });
        var (repo, _) = Build(store, shufflerMock);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 3 }, CancellationToken.None);

        // Assert — should return 3 items in reversed order [5,4,3]
        Assert.Equal(3, result.Count);
        Assert.Equal([5, 4, 3], result.Select(q => q.Id).ToArray());
    }

    // -------------------------------------------------------------------------
    // GetAsync — Seam exceptions bubble out (not swallowed)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_Bubbles_WhenSelectCandidatesThrowsIOException()
    {
        // Arrange
        var store = new Mock<IQuestionStore>();
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Throws(new IOException("disk error"));
        var (repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            () => repo.GetAsync(new QuestionQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_Bubbles_WhenFetchByIdsThrowsInvalidOperationException()
    {
        // Arrange
        var store = new Mock<IQuestionStore>();
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Returns(_threeCandidates);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Throws(new InvalidOperationException("unexpected state"));
        var (repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None));
    }

    [Fact]
    public void Constructor_Bubbles_WhenLoadSummaryThrowsInvalidOperationException()
    {
        // Arrange
        var store = new Mock<IQuestionStore>();
        store.Setup(s => s.LoadSummary())
            .Throws(new InvalidOperationException("schema mismatch"));
        var shuffler = new Mock<IShuffler>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => new SqliteQuestionRepository(
                store.Object,
                shuffler.Object,
                new CapturingLogger<SqliteQuestionRepository>()));
    }

    // -------------------------------------------------------------------------
    // GetAsync — Unknown-value warnings
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_LogsWarning_WhenStageIsNotInKnownSet()
    {
        // Arrange — known stages are E1, E2 (from DefaultSummary)
        var store = StoreReturningCandidates([]);
        var (repo, logger) = Build(store);
        var query = new QuestionQuery { Stages = ["UNKNOWN_STAGE"] };

        // Act
        await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Contains(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("UNKNOWN_STAGE"));
    }

    [Fact]
    public async Task GetAsync_LogsWarning_WhenYearIsNotInKnownSet()
    {
        // Arrange — known years are 2023, 2024
        var store = StoreReturningCandidates([]);
        var (repo, logger) = Build(store);
        var query = new QuestionQuery { Years = [1999] };

        // Act
        await repo.GetAsync(query, CancellationToken.None);

        // Assert
        Assert.Contains(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("1999"));
    }

    [Fact]
    public async Task GetAsync_DoesNotLogWarning_WhenAllValuesAreKnown()
    {
        // Arrange
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, logger) = Build(store);
        var query = new QuestionQuery { Stages = ["E1"], Years = [2023] };

        // Act
        await repo.GetAsync(query, CancellationToken.None);

        // Assert — no warning-level entries about unknown values
        Assert.DoesNotContain(logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("not present"));
    }

    // -------------------------------------------------------------------------
    // Row-to-Question mapping
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_MapsAllFieldsCorrectly_FromRowToQuestion()
    {
        // Arrange
        QuestionRow fullRow = new()
        {
            Id                = 42,
            Olympiad          = "OIJ",
            Stage             = "E2",
            Year              = 2025,
            Difficulty        = 3,
            Source            = "Olimpiada 2025",
            SourceUrl         = "https://example.com/task",
            SourceRaw         = "Source raw text",
            ExplanationSource = "Explanation source",
            Type              = "multi",
            Content           = "[{\"Type\":\"text\",\"Text\":\"Which?\"}]",
            Options           = "[\"A\",\"B\",\"C\"]",
            CorrectAnswer     = "[\"A\",\"C\"]",
            Category          = "[\"grafy\"]",
            Algorithms        = "[\"DFS\"]",
            Points            = 2,
            PartialCredit     = 1
        };

        var store = new Mock<IQuestionStore>();
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Returns([new QuestionCandidate { Id = 42, Category = "[\"grafy\"]", Algorithms = "[\"DFS\"]" }]);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Returns([fullRow]);
        var (repo, _) = Build(store);

        // Act
        IReadOnlyList<Question> result = await repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None);

        // Assert
        Question q = Assert.Single(result);
        Assert.Equal(42, q.Id);
        Assert.Equal("OIJ", q.Olympiad);
        Assert.Equal("E2", q.Stage);
        Assert.Equal(2025, q.Year);
        Assert.Equal(3, q.Difficulty);
        Assert.Equal("Olimpiada 2025", q.Source);
        Assert.Equal("https://example.com/task", q.SourceUrl);
        Assert.Equal("Source raw text", q.SourceRaw);
        Assert.Equal("Explanation source", q.ExplanationSource);
        Assert.Equal(QuestionType.Multi, q.Type);
        Assert.Equal(["A", "B", "C"], q.Options);
        Assert.Equal(["A", "C"], q.CorrectAnswer);
        Assert.Equal(["grafy"], q.Category);
        Assert.Equal(["DFS"], q.Algorithms);
        Assert.Equal(2, q.Points);
        Assert.True(q.PartialCredit);
    }

    [Fact]
    public async Task GetAsync_ThrowsInvalidOperationException_WhenRowHasUnrecognisedTypeString()
    {
        // Arrange — FetchByIds returns a row with type="bogus"
        var store = new Mock<IQuestionStore>();
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        store.Setup(s => s.SelectCandidates(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<IReadOnlyCollection<int>>()))
            .Returns([new QuestionCandidate { Id = 1, Category = "[]", Algorithms = "[]" }]);
        store.Setup(s => s.FetchByIds(It.IsAny<IReadOnlyList<int>>()))
            .Returns([MinimalRow(1, type: "bogus")]);
        var (repo, _) = Build(store);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.GetAsync(new QuestionQuery { Limit = 10 }, CancellationToken.None));
    }

    // -------------------------------------------------------------------------
    // GetFilterOptionsAsync — returns cached; no extra store calls
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsCachedOptions_WithoutCallingStore()
    {
        // Arrange
        var store = new Mock<IQuestionStore>(MockBehavior.Strict);
        store.Setup(s => s.LoadSummary()).Returns(DefaultSummary());
        var (repo, _) = Build(store);
        // Strict mock: any additional store call after construction will throw

        // Act
        FilterOptions options = await repo.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, options.TotalQuestions);
        Assert.Equal(2, options.Stages.Count);
        Assert.Equal(2, options.Years.Count);
        Assert.Equal(2, options.Categories.Count);
        Assert.Equal(2, options.Algorithms.Count);
        store.Verify(s => s.LoadSummary(), Times.Once); // only from constructor
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ReturnsSameInstance_OnMultipleCalls()
    {
        // Arrange
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);

        // Act
        FilterOptions first  = await repo.GetFilterOptionsAsync(CancellationToken.None);
        FilterOptions second = await repo.GetFilterOptionsAsync(CancellationToken.None);

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public async Task GetFilterOptionsAsync_ThrowsOperationCanceledException_WhenTokenAlreadyCancelled()
    {
        // Arrange
        var store = StoreReturningCandidates(_threeCandidates);
        var (repo, _) = Build(store);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.GetFilterOptionsAsync(cts.Token));
    }
}
