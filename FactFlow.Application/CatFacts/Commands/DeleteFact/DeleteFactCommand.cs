using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.DeleteFact;

public sealed record DeleteFactCommand(int Id) : ICommand<bool>;
