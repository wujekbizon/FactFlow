namespace FactFlow.Application.CatFacts.Queries.GetDeletionRequests;

public sealed record DeletionRequestItem(
    int Id,
    int FactId,
    string FactContentSnapshot,
    string Status,
    string RequestedBy,
    string Reason,
    DateTimeOffset RequestedAtUtc,
    string? DecidedBy,
    string? DecisionNote,
    DateTimeOffset? DecidedAtUtc);
