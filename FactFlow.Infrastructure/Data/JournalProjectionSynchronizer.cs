using FactFlow.Application.Abstractions;
using FactFlow.Domain.CatFacts;
using Microsoft.Extensions.Logging;

namespace FactFlow.Infrastructure.Data;

public sealed class JournalProjectionSynchronizer(
    IFactJournal factJournal,
    IFactRepository factRepository,
    ILogger<JournalProjectionSynchronizer> logger)
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        var journalFacts = await factJournal.ReadAllAsync(cancellationToken);
        var persistedSequences = await factRepository.GetJournalSequencesAsync(cancellationToken);
        var imported = 0;

        for (var index = 0; index < journalFacts.Count; index++)
        {
            var sequence = index + 1;
            if (persistedSequences.Contains(sequence))
            {
                continue;
            }

            var record = FactRecord.Create(
                journalFacts[index],
                FactSource.JournalImport,
                sequence,
                DateTimeOffset.UtcNow.AddTicks(sequence));
            await factRepository.AddAsync(record, cancellationToken);
            imported++;
        }

        if (imported == 0)
        {
            return;
        }

        await factRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Imported {Count} missing journal records into SQL Server", imported);
    }
}
