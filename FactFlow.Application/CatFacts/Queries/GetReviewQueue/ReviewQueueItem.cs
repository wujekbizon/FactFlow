namespace FactFlow.Application.CatFacts.Queries.GetReviewQueue;

public sealed record ReviewQueueItem(
    int Id,
    string Content,
    int Length,
    string Source,
    int JournalSequence,
    string ReviewStatus,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset CreatedAtUtc);
