namespace FactFlow.Application.CatFacts.Queries.GetFactById;

public sealed record FactDetails(
    int Id,
    string Content,
    int Length,
    string Source,
    int JournalSequence,
    string ReviewStatus,
    string? ReviewedBy,
    string? ReviewNote,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    bool HasPendingDeletionRequest,
    IReadOnlyList<FactReviewAuditItem> ReviewHistory);
