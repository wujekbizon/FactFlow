namespace FactFlow.Application.CatFacts.Queries.GetFactById;

public sealed record FactDetails(
    int Id,
    string Content,
    int Length,
    string Source,
    int JournalSequence,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
