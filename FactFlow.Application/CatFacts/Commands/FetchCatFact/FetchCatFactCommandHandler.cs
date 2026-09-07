using System.Diagnostics;
using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.FetchCatFact;

public sealed class FetchCatFactCommandHandler(
    ICatFactClient catFactClient,
    IFactJournal factJournal,
    IFactRepository factRepository)
    : ICommandHandler<FetchCatFactCommand, FetchCatFactResult>
{
    public async Task<FetchCatFactResult> Handle(
        FetchCatFactCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var fact = await catFactClient.GetFactAsync(cancellationToken);

        if (!fact.IsValid)
        {
            throw new InvalidDataException("The Cat Fact API returned an invalid response.");
        }

        var sequence = await factJournal.AppendAsync(fact, cancellationToken);
        var record = FactRecord.Create(fact, FactSource.Api, sequence, DateTimeOffset.UtcNow);
        await factRepository.AddAsync(record, cancellationToken);
        await factRepository.SaveChangesAsync(cancellationToken);
        stopwatch.Stop();

        return new FetchCatFactResult(record.Id, fact, stopwatch.ElapsedMilliseconds, factJournal.FilePath);
    }
}
