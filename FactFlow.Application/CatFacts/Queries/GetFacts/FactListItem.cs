namespace FactFlow.Application.CatFacts.Queries.GetFacts;

public sealed record FactListItem(
    int Id,
    string Content,
    int Length,
    string Source,
    int JournalSequence,
    string ReviewStatus,
    bool HasPendingDeletionRequest,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
