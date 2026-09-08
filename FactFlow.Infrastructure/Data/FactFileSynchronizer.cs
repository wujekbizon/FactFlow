using FactFlow.Application.Abstractions;
using FactFlow.Domain.CatFacts;
using Microsoft.Extensions.Logging;

namespace FactFlow.Infrastructure.Data;

public sealed class FactFileSynchronizer(
    IFactJournal factJournal,
    IFactRepository factRepository,
    ILogger<FactFileSynchronizer> logger) : IFactFileSynchronizer
{
    public async Task BootstrapDatabaseFromFileIfEmptyAsync(
        CancellationToken cancellationToken = default)
    {
        var persistedSequences = await factRepository.GetJournalSequencesAsync(cancellationToken);
        if (persistedSequences.Count > 0)
        {
            return;
        }

        var fileFacts = await factJournal.ReadAllAsync(cancellationToken);
        for (var index = 0; index < fileFacts.Count; index++)
        {
            var sequence = index + 1;
            var record = FactRecord.Create(
                fileFacts[index],
                FactSource.JournalImport,
                sequence,
                DateTimeOffset.UtcNow.AddTicks(sequence));
            await factRepository.AddAsync(record, cancellationToken);
        }

        if (fileFacts.Count > 0)
        {
            await factRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Bootstrapped SQL Server from {Count} TXT records", fileFacts.Count);
        }
    }

    public async Task SynchronizeFromDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var records = await factRepository.ListAsync(cancellationToken);
        var fileFacts = records
            .OrderBy(record => record.JournalSequence)
            .Select(record => new CatFact(record.Content, record.Length))
            .ToArray();

        await factJournal.ReplaceAllAsync(fileFacts, cancellationToken);
    }
}
