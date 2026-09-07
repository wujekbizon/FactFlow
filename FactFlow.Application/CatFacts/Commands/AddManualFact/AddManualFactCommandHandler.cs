using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.AddManualFact;

public sealed class AddManualFactCommandHandler(IFactJournal factJournal)
    : ICommandHandler<AddManualFactCommand, CatFact>
{
    public async Task<CatFact> Handle(
        AddManualFactCommand command,
        CancellationToken cancellationToken)
    {
        var content = command.Fact.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Fact content is required.", nameof(command));
        }

        var fact = new CatFact(content, content.Length);
        await factJournal.AppendAsync(fact, cancellationToken);
        return fact;
    }
}
