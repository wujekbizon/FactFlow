using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetReviewQueue;

public sealed record GetReviewQueueQuery(string? Status, string? Source, string? Reviewer)
    : IQuery<ReviewQueueSnapshot>;
