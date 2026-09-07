using System.Diagnostics;
using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.FetchCatFact;

public sealed class FetchCatFactCommandHandler(
    ICatFactClient catFactClient,
    IFactJournal factJournal)
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

        await factJournal.AppendAsync(fact, cancellationToken);
        stopwatch.Stop();

        return new FetchCatFactResult(fact, stopwatch.ElapsedMilliseconds, factJournal.FilePath);
    }
}
