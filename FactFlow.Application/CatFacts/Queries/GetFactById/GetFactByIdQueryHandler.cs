using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactById;

public sealed class GetFactByIdQueryHandler(IFactRepository factRepository)
    : IQueryHandler<GetFactByIdQuery, FactDetails?>
{
    public async Task<FactDetails?> Handle(GetFactByIdQuery query, CancellationToken cancellationToken)
    {
        var fact = await factRepository.FindAsync(query.Id, cancellationToken: cancellationToken);
        if (fact is null)
        {
            return null;
        }

        var history = await factRepository.ListReviewAuditAsync(fact.Id, cancellationToken);
        var pendingDeletionFactIds = await factRepository.GetPendingDeletionFactIdsAsync(cancellationToken);
        return new FactDetails(
            fact.Id,
            fact.Content,
            fact.Length,
            fact.Source.ToString(),
            fact.JournalSequence,
            fact.ReviewStatus.ToString(),
            fact.ReviewedBy,
            fact.ReviewNote,
            fact.ReviewedAtUtc,
            fact.CreatedAtUtc,
            fact.UpdatedAtUtc,
            pendingDeletionFactIds.Contains(fact.Id),
            history.Select(entry => new FactReviewAuditItem(
                entry.Id,
                entry.FromStatus.ToString(),
                entry.ToStatus.ToString(),
                entry.ReviewedBy,
                entry.Note,
                entry.OccurredAtUtc)).ToArray());
    }
}
