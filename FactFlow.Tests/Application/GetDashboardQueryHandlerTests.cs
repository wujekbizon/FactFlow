using FactFlow.Application.Abstractions;
using FactFlow.Application.CatFacts.Queries.GetDashboard;
using FactFlow.Domain.CatFacts;
using FactFlow.Tests.TestDoubles;

namespace FactFlow.Tests.Application;

public sealed class GetDashboardQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesMetricsAndReturnsNewestFirst()
    {
        CatFact[] journalFacts =
        [
            new("First fact", 10),
            new("Second fact", 20),
            new("First fact", 10)
        ];
        var records = journalFacts
            .Select((fact, index) => FactRecord.Create(fact, FactSource.JournalImport, index + 1, DateTimeOffset.UtcNow))
            .Reverse()
            .ToArray();
        var handler = new GetDashboardQueryHandler(
            new StubFactJournal(journalFacts),
            new InMemoryFactRepository(records));

        var result = await handler.Handle(new GetDashboardQuery(), CancellationToken.None);

        Assert.Equal(3, result.TotalRequests);
        Assert.Equal(2, result.UniqueFacts);
        Assert.Equal(40d / 3d, result.AverageLength, precision: 6);
        Assert.Equal(20, result.LongestFactLength);
        Assert.Equal(3, result.RecentFacts[0].Sequence);
    }

    private sealed class StubFactJournal(IReadOnlyList<CatFact> facts) : IFactJournal
    {
        public string FilePath => "facts.txt";
        public Task<int> AppendAsync(CatFact fact, CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(facts);
        public Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }
}
