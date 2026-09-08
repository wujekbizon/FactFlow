namespace FactFlow.Application.Security;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The record was changed by another user. Refresh and try again.")
    {
    }

    public ConcurrencyConflictException(Exception inner)
        : base("The record was changed by another user. Refresh and try again.", inner)
    {
    }
}
