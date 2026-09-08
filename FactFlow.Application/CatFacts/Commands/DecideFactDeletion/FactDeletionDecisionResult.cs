namespace FactFlow.Application.CatFacts.Commands.DecideFactDeletion;

public enum FactDeletionDecisionOutcome
{
    Approved,
    ApprovedWithProjectionWarning,
    Rejected,
    Expired,
    NotFound,
    Forbidden,
    AlreadyDecided,
    Conflict,
    InvalidInput
}

public sealed record FactDeletionDecisionResult(
    FactDeletionDecisionOutcome Outcome,
    string Message)
{
    public bool Success => Outcome is
        FactDeletionDecisionOutcome.Approved or
        FactDeletionDecisionOutcome.ApprovedWithProjectionWarning or
        FactDeletionDecisionOutcome.Rejected;
}
