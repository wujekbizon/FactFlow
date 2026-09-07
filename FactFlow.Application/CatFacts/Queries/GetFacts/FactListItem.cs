namespace FactFlow.Application.CatFacts.Queries.GetFacts;

public sealed record FactListItem(
    int Id,
    string Content,
    int Length,
    string Source,
    int JournalSequence,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
