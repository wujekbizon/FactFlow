using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler(IFactJournal factJournal, IFactRepository factRepository)
    : IQueryHandler<GetDashboardQuery, DashboardSnapshot>
{
    public async Task<DashboardSnapshot> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var facts = await factRepository.ListAsync(cancellationToken);
        var recent = facts
            .Select(fact => new DashboardFactItem(fact.JournalSequence, fact.Content, fact.Length))
            .Take(10)
            .ToArray();

        return new DashboardSnapshot(
            TotalRequests: facts.Count,
            UniqueFacts: facts.Select(fact => fact.Content).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            AverageLength: facts.Count == 0 ? 0 : facts.Average(fact => fact.Length),
            LongestFactLength: facts.Count == 0 ? 0 : facts.Max(fact => fact.Length),
            JournalPath: factJournal.FilePath,
            RecentFacts: recent);
    }
}
