namespace FactFlow.Application.Security;

public sealed class DuplicatePendingDeletionRequestException : Exception
{
    public DuplicatePendingDeletionRequestException(Exception inner)
        : base("A deletion request is already pending for this fact.", inner)
    {
    }
}
