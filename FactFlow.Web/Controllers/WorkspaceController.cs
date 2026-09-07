using FactFlow.Web.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactFlow.Web.Controllers;

[Authorize]
public sealed class WorkspaceController(IWorkspaceTabService workspaceTabs) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Close(string tabId)
    {
        var next = workspaceTabs.Close(tabId);
        return RedirectToAction(
            next.Action,
            next.Controller,
            next.Id == "dashboard" ? null : new { id = next.EntityId, tabId = next.Id });
    }
}
