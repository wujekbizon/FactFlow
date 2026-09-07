using FactFlow.Application.Abstractions;
using FactFlow.Application.CatFacts.Commands.AddManualFact;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Tests.Application;

public sealed class AddManualFactCommandHandlerTests
{
    [Fact]
    public async Task Handle_TrimsContentCalculatesLengthAndAppends()
    {
        var journal = new RecordingJournal();
        var handler = new AddManualFactCommandHandler(journal);

        var result = await handler.Handle(new AddManualFactCommand("  Manual cat fact.  "), CancellationToken.None);

        Assert.Equal("Manual cat fact.", result.Fact);
        Assert.Equal(result.Fact.Length, result.Length);
        Assert.Equal(result, Assert.Single(journal.Facts));
    }

    private sealed class RecordingJournal : IFactJournal
    {
        public List<CatFact> Facts { get; } = [];
        public string FilePath => "facts.txt";
        public Task AppendAsync(CatFact fact, CancellationToken cancellationToken = default) { Facts.Add(fact); return Task.CompletedTask; }
        public Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CatFact>>(Facts);
        public Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }
}
