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
        if (next.Id == "dashboard")
        {
            return RedirectToAction(next.Action, next.Controller);
        }

        if (next.Kind == "DeletionRequest")
        {
            return RedirectToAction(
                next.Action,
                next.Controller,
                new { factId = next.EntityId, tabId = next.Id });
        }

        return RedirectToAction(
            next.Action,
            next.Controller,
            new { id = next.EntityId, tabId = next.Id });
    }
}
