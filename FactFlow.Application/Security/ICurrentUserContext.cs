namespace FactFlow.Application.Security;

public interface ICurrentUserContext
{
    string UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
