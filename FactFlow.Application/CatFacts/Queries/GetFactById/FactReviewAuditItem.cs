namespace FactFlow.Application.CatFacts.Queries.GetFactById;

public sealed record FactReviewAuditItem(
    int Id,
    string FromStatus,
    string ToStatus,
    string ReviewedBy,
    string? Note,
    DateTimeOffset OccurredAtUtc);
