namespace OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

public interface IQuestionStore
{
    // Returns id + tag JSON columns for questions matching the indexed predicates.
    // Tag matching (category, algorithms) happens in application code above this seam.
    IReadOnlyList<QuestionCandidate> SelectCandidates(
        IReadOnlyCollection<string> stages, IReadOnlyCollection<int> years);

    // Fetches full rows for the given identifiers only.
    IReadOnlyList<QuestionRow> FetchByIds(IReadOnlyList<int> ids);

    // Loads bank-level aggregates used at construction for filter options and known-value sets.
    BankSummary LoadSummary();
}
