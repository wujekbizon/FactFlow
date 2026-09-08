using FactFlow.Application.CatFacts.Commands.DeleteFact;
using FactFlow.Application.CatFacts.Commands.UpdateFact;
using FactFlow.Domain.CatFacts;
using FactFlow.Tests.TestDoubles;

namespace FactFlow.Tests.Application;

public sealed class FactCrudCommandHandlerTests
{
    [Fact]
    public async Task Update_ChangesContentAndCalculatedLength()
    {
        var fact = FactRecord.Create(new CatFact("Original fact", 13), FactSource.Manual, 1, DateTimeOffset.UtcNow);
        var repository = new InMemoryFactRepository([fact]);
        var synchronizer = new RecordingFactFileSynchronizer();
        var handler = new UpdateFactCommandHandler(repository, synchronizer);

        var updated = await handler.Handle(new UpdateFactCommand(0, " Updated fact "), CancellationToken.None);

        Assert.True(updated);
        Assert.Equal("Updated fact", fact.Content);
        Assert.Equal(12, fact.Length);
        Assert.Equal(1, repository.SaveCount);
        Assert.Equal(1, synchronizer.SynchronizationCount);
    }

    [Fact]
    public async Task Delete_RemovesDatabaseRecord()
    {
        var fact = FactRecord.Create(new CatFact("Delete fact", 11), FactSource.Manual, 1, DateTimeOffset.UtcNow);
        var repository = new InMemoryFactRepository([fact]);
        var synchronizer = new RecordingFactFileSynchronizer();
        var handler = new DeleteFactCommandHandler(repository, synchronizer);

        var deleted = await handler.Handle(new DeleteFactCommand(0), CancellationToken.None);

        Assert.True(deleted);
        Assert.True(fact.IsDeleted);
        Assert.NotNull(fact.DeletedAtUtc);
        Assert.Empty(await repository.ListAsync());
        Assert.Equal(1, repository.SaveCount);
        Assert.Equal(1, synchronizer.SynchronizationCount);
    }
}
