using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Commands.DecideFactDeletion;

public sealed record ApproveFactDeletionCommand(int RequestId, string? Note)
    : ICommand<FactDeletionDecisionResult>;

public sealed record RejectFactDeletionCommand(int RequestId, string Note)
    : ICommand<FactDeletionDecisionResult>;
