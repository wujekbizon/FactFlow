using FactFlow.Web.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactFlow.Web.Controllers;

[Authorize]
public sealed class WorkspaceController(IWorkspaceTabService workspaceTabs) : Controller
{
    [HttpGet]
    public IActionResult Cancel(string tabId)
    {
        return RedirectToNext(workspaceTabs.Close(tabId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Close(string tabId)
    {
        return RedirectToNext(workspaceTabs.Close(tabId));
    }

    private IActionResult RedirectToNext(WorkspaceTab next)
    {
        return RedirectToAction(
            next.Action,
            next.Controller,
            next.Id == "dashboard" ? null : new { id = next.EntityId, tabId = next.Id });
    }
}
