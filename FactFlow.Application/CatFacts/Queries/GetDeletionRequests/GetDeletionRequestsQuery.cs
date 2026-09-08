using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetDeletionRequests;

public sealed record GetDeletionRequestsQuery(string? Status)
    : IQuery<DeletionRequestQueueSnapshot>;
