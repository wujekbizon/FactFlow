using FactFlow.Application.Security;
using System.Security.Claims;

namespace FactFlow.Web.Authentication;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserContext
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public string UserName => User.Identity?.Name ?? string.Empty;
    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;
    public bool IsInRole(string role) => User.IsInRole(role);
}
