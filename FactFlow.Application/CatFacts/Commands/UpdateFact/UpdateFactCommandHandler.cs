using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.UpdateFact;

public sealed class UpdateFactCommandHandler(
    IFactRepository factRepository,
    IFactFileSynchronizer factFileSynchronizer)
    : ICommandHandler<UpdateFactCommand, bool>
{
    public async Task<bool> Handle(UpdateFactCommand command, CancellationToken cancellationToken)
    {
        var fact = await factRepository.FindAsync(command.Id, trackChanges: true, cancellationToken);
        if (fact is null)
        {
            return false;
        }

        fact.UpdateContent(command.Content, DateTimeOffset.UtcNow);
        await factRepository.SaveChangesAsync(cancellationToken);
        await factFileSynchronizer.SynchronizeFromDatabaseAsync(cancellationToken);
        return true;
    }
}
