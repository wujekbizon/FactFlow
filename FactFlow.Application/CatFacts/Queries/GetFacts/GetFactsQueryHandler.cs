using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFacts;

public sealed class GetFactsQueryHandler(IFactRepository factRepository)
    : IQueryHandler<GetFactsQuery, IReadOnlyList<FactListItem>>
{
    public async Task<IReadOnlyList<FactListItem>> Handle(
        GetFactsQuery query,
        CancellationToken cancellationToken)
    {
        var facts = await factRepository.ListAsync(cancellationToken);
        var pendingDeletionFactIds = await factRepository.GetPendingDeletionFactIdsAsync(cancellationToken);
        return facts
            .Select(fact => new FactListItem(
                fact.Id,
                fact.Content,
                fact.Length,
                fact.Source.ToString(),
                fact.JournalSequence,
                fact.ReviewStatus.ToString(),
                pendingDeletionFactIds.Contains(fact.Id),
                fact.CreatedAtUtc,
                fact.UpdatedAtUtc))
            .ToArray();
    }
}
