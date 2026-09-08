using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.RequestFactDeletion;

public sealed record RequestFactDeletionCommand(int FactId, string Reason)
    : ICommand<RequestFactDeletionResult>;
