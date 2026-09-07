using FactFlow.Application.Abstractions;
using FactFlow.Application.CatFacts.Queries.GetFactHistory;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Tests.Application;

public sealed class GetFactHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesIntegrityDuplicatesHashAndRawJson()
    {
        CatFact[] facts = [new("Same fact", 9), new("Same fact", 99), new("Unique", 6)];
        var handler = new GetFactHistoryQueryHandler(new StubJournal(facts));

        var result = await handler.Handle(new GetFactHistoryQuery(), CancellationToken.None);

        Assert.Equal(3, result.TotalRecords);
        Assert.Equal(2, result.UniqueRecords);
        Assert.Equal(2, result.DuplicateRecords);
        Assert.Equal(1, result.IntegrityFailures);
        Assert.All(result.Records, record => Assert.Equal(64, record.Sha256.Length));
        Assert.Contains("\"fact\"", result.Records[0].RawJson);
    }

    private sealed class StubJournal(IReadOnlyList<CatFact> facts) : IFactJournal
    {
        public string FilePath => "facts.txt";
        public Task<int> AppendAsync(CatFact fact, CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(facts);
        public Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }
}
