using FactFlow.Application.Abstractions;
using FactFlow.Application.CatFacts.Commands.AddManualFact;
using FactFlow.Domain.CatFacts;
using FactFlow.Tests.TestDoubles;

namespace FactFlow.Tests.Application;

public sealed class AddManualFactCommandHandlerTests
{
    [Fact]
    public async Task Handle_TrimsContentCalculatesLengthAndAppends()
    {
        var journal = new RecordingJournal();
        var repository = new InMemoryFactRepository();
        var handler = new AddManualFactCommandHandler(journal, repository);

        var result = await handler.Handle(new AddManualFactCommand("  Manual cat fact.  "), CancellationToken.None);

        Assert.Equal("Manual cat fact.", result.Content);
        Assert.Equal(result.Content.Length, result.Length);
        Assert.Equal("Manual cat fact.", Assert.Single(journal.Facts).Fact);
        Assert.Equal(FactSource.Manual, Assert.Single(repository.Facts).Source);
    }

    private sealed class RecordingJournal : IFactJournal
    {
        public List<CatFact> Facts { get; } = [];
        public string FilePath => "facts.txt";
        public Task<int> AppendAsync(CatFact fact, CancellationToken cancellationToken = default) { Facts.Add(fact); return Task.FromResult(Facts.Count); }
        public Task ReplaceAllAsync(IReadOnlyList<CatFact> facts, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CatFact>>(Facts);
        public Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }
}
