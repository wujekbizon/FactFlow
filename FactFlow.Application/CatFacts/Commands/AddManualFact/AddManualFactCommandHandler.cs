using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.AddManualFact;

public sealed class AddManualFactCommandHandler(IFactJournal factJournal, IFactRepository factRepository)
    : ICommandHandler<AddManualFactCommand, FactRecord>
{
    public async Task<FactRecord> Handle(
        AddManualFactCommand command,
        CancellationToken cancellationToken)
    {
        var content = command.Fact.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Fact content is required.", nameof(command));
        }

        var fact = new CatFact(content, content.Length);
        var sequence = await factJournal.AppendAsync(fact, cancellationToken);
        var record = FactRecord.Create(fact, FactSource.Manual, sequence, DateTimeOffset.UtcNow);
        await factRepository.AddAsync(record, cancellationToken);
        await factRepository.SaveChangesAsync(cancellationToken);
        return record;
    }
}
