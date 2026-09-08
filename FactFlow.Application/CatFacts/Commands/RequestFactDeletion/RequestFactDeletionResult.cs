namespace FactFlow.Application.CatFacts.Commands.RequestFactDeletion;

public enum RequestFactDeletionOutcome
{
    Created,
    NotFound,
    Forbidden,
    AlreadyPending,
    InvalidReason
}

public sealed record RequestFactDeletionResult(
    RequestFactDeletionOutcome Outcome,
    string Message,
    int? RequestId = null)
{
    public bool Success => Outcome == RequestFactDeletionOutcome.Created;
}
