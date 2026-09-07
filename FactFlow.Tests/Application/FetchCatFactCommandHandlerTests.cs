using FactFlow.Application.Abstractions;
using FactFlow.Application.CatFacts.Commands.FetchCatFact;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Tests.Application;

public sealed class FetchCatFactCommandHandlerTests
{
    [Fact]
    public async Task Handle_AppendsReceivedFactAndReturnsResult()
    {
        var expected = new CatFact("Cats sleep for much of the day.", 32);
        var client = new StubCatFactClient(expected);
        var journal = new RecordingFactJournal();
        var handler = new FetchCatFactCommandHandler(client, journal);

        var result = await handler.Handle(new FetchCatFactCommand(), CancellationToken.None);

        Assert.Equal(expected, result.Fact);
        Assert.Equal("test-journal.txt", result.JournalPath);
        Assert.Equal([expected], journal.AppendedFacts);
    }

    [Fact]
    public async Task Handle_InvalidResponse_DoesNotAppend()
    {
        var client = new StubCatFactClient(new CatFact(string.Empty, 0));
        var journal = new RecordingFactJournal();
        var handler = new FetchCatFactCommandHandler(client, journal);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => handler.Handle(new FetchCatFactCommand(), CancellationToken.None));

        Assert.Empty(journal.AppendedFacts);
    }

    private sealed class StubCatFactClient(CatFact fact) : ICatFactClient
    {
        public Task<CatFact> GetFactAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(fact);
    }

    private sealed class RecordingFactJournal : IFactJournal
    {
        public List<CatFact> AppendedFacts { get; } = [];
        public string FilePath => "test-journal.txt";

        public Task AppendAsync(CatFact fact, CancellationToken cancellationToken = default)
        {
            AppendedFacts.Add(fact);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CatFact>>(AppendedFacts);

        public Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);
    }
}
