using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.DeleteFact;

public sealed class DeleteFactCommandHandler(
    IFactRepository factRepository,
    IFactFileSynchronizer factFileSynchronizer)
    : ICommandHandler<DeleteFactCommand, bool>
{
    public async Task<bool> Handle(DeleteFactCommand command, CancellationToken cancellationToken)
    {
        var fact = await factRepository.FindAsync(command.Id, trackChanges: true, cancellationToken);
        if (fact is null)
        {
            return false;
        }

        fact.SoftDelete(DateTimeOffset.UtcNow);
        await factRepository.SaveChangesAsync(cancellationToken);
        await factFileSynchronizer.SynchronizeFromDatabaseAsync(cancellationToken);
        return true;
    }
}
