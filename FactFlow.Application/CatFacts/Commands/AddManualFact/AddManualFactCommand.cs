using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.AddManualFact;

public sealed record AddManualFactCommand(string Fact) : ICommand<CatFact>;
