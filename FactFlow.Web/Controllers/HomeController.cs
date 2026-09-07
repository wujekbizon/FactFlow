using System.Diagnostics;
using FactFlow.Application.CatFacts.Queries.GetDashboard;
using FactFlow.Application.Common.Messaging;
using FactFlow.Web.Models;
using FactFlow.Web.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactFlow.Web.Controllers;

[Authorize]
public class HomeController(
    IQueryHandler<GetDashboardQuery, DashboardSnapshot> dashboardQuery,
    IWorkspaceTabService workspaceTabs) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        workspaceTabs.ActivateDashboard();
        var dashboard = await dashboardQuery.Handle(new GetDashboardQuery(), cancellationToken);
        return View(dashboard);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
