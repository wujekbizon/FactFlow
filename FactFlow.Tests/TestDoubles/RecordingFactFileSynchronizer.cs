using FactFlow.Application.Abstractions;

namespace FactFlow.Tests.TestDoubles;

internal sealed class RecordingFactFileSynchronizer : IFactFileSynchronizer
{
    public int SynchronizationCount { get; private set; }

    public Task SynchronizeFromDatabaseAsync(CancellationToken cancellationToken = default)
    {
        SynchronizationCount++;
        return Task.CompletedTask;
    }
}
