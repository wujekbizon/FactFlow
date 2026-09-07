using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.FetchCatFact;

public sealed record FetchCatFactCommand : ICommand<FetchCatFactResult>;
