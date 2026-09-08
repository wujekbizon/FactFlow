using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FactFlow.Web.Authentication;
using FactFlow.Application.Security;
using FactFlow.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FactFlow.Web.Controllers;

public sealed class AccountController(IOptions<DemoAuthOptions> authOptions) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginInputModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var configured = authOptions.Value;
        var operatorConfigured = !string.IsNullOrWhiteSpace(configured.Username)
            && !string.IsNullOrEmpty(configured.Password);
        var supervisorConfigured = !string.IsNullOrWhiteSpace(configured.SupervisorUsername)
            && !string.IsNullOrEmpty(configured.SupervisorPassword);
        if (!operatorConfigured && !supervisorConfigured)
        {
            ModelState.AddModelError(string.Empty, "Login is not configured for this environment.");
            return View(model);
        }

        var isOperator = operatorConfigured
            && string.Equals(model.Username, configured.Username, StringComparison.OrdinalIgnoreCase)
            && FixedTimeEquals(model.Password, configured.Password);
        var isSupervisor = supervisorConfigured
            && string.Equals(model.Username, configured.SupervisorUsername, StringComparison.OrdinalIgnoreCase)
            && FixedTimeEquals(model.Password, configured.SupervisorPassword);

        if (!isOperator && !isSupervisor)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        Claim[] claims =
        [
            new(ClaimTypes.Name, isSupervisor ? configured.SupervisorUsername : configured.Username),
            new(ClaimTypes.Role, isSupervisor ? AppRoles.Supervisor : AppRoles.Operator)
        ];
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl)
            : RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private static bool FixedTimeEquals(string supplied, string expected)
    {
        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(suppliedHash, expectedHash);
    }
}
