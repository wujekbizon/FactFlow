namespace FactFlow.Application.CatFacts.Queries.GetFactHistory;

public sealed record FactHistorySnapshot(
    int TotalRecords,
    int UniqueRecords,
    int DuplicateRecords,
    int IntegrityFailures,
    string JournalPath,
    IReadOnlyList<FactHistoryItem> Records);
