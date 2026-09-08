using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Queries.GetReviewQueue;

public sealed class GetReviewQueueQueryHandler(IFactRepository factRepository)
    : IQueryHandler<GetReviewQueueQuery, ReviewQueueSnapshot>
{
    public async Task<ReviewQueueSnapshot> Handle(
        GetReviewQueueQuery query,
        CancellationToken cancellationToken)
    {
        var facts = await factRepository.ListAsync(cancellationToken);
        var filtered = facts.AsEnumerable();

        if (Enum.TryParse<FactReviewStatus>(query.Status, true, out var status))
        {
            filtered = filtered.Where(fact => fact.ReviewStatus == status);
        }

        if (Enum.TryParse<FactSource>(query.Source, true, out var source))
        {
            filtered = filtered.Where(fact => fact.Source == source);
        }

        if (!string.IsNullOrWhiteSpace(query.Reviewer))
        {
            filtered = filtered.Where(fact => string.Equals(
                fact.ReviewedBy,
                query.Reviewer,
                StringComparison.OrdinalIgnoreCase));
        }

        var items = filtered
            .OrderBy(fact => fact.ReviewStatus)
            .ThenByDescending(fact => fact.CreatedAtUtc)
            .Select(fact => new ReviewQueueItem(
                fact.Id,
                fact.Content,
                fact.Length,
                fact.Source.ToString(),
                fact.JournalSequence,
                fact.ReviewStatus.ToString(),
                fact.ReviewedBy,
                fact.ReviewedAtUtc,
                fact.CreatedAtUtc))
            .ToArray();

        return new ReviewQueueSnapshot(
            facts.Count,
            facts.Count(fact => fact.ReviewStatus == FactReviewStatus.New),
            facts.Count(fact => fact.ReviewStatus == FactReviewStatus.Reviewed),
            facts.Count(fact => fact.ReviewStatus == FactReviewStatus.Approved),
            facts.Count(fact => fact.ReviewStatus == FactReviewStatus.Rejected),
            query.Status,
            query.Source,
            query.Reviewer,
            facts.Where(fact => !string.IsNullOrWhiteSpace(fact.ReviewedBy))
                .Select(fact => fact.ReviewedBy!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(reviewer => reviewer)
                .ToArray(),
            items);
    }
}
