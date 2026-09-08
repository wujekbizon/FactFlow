namespace FactFlow.Application.Abstractions;

public interface IFactFileSynchronizer
{
    Task SynchronizeFromDatabaseAsync(CancellationToken cancellationToken = default);
}
