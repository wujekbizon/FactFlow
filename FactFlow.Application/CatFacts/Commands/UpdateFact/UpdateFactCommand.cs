using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.UpdateFact;

public sealed record UpdateFactCommand(int Id, string Content) : ICommand<bool>;
