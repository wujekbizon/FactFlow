using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.FetchCatFact;

public sealed record FetchCatFactResult(
    CatFact Fact,
    long DurationMilliseconds,
    string JournalPath);
