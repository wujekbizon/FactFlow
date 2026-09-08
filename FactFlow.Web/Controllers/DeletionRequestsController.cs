using FactFlow.Application.CatFacts.Commands.DecideFactDeletion;
using FactFlow.Application.CatFacts.Commands.RequestFactDeletion;
using FactFlow.Application.CatFacts.Queries.GetDeletionRequests;
using FactFlow.Application.CatFacts.Queries.GetFactById;
using FactFlow.Application.Common.Messaging;
using FactFlow.Application.Security;
using FactFlow.Web.Models;
using FactFlow.Web.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactFlow.Web.Controllers;

[Authorize]
public sealed class DeletionRequestsController(
    ICommandHandler<RequestFactDeletionCommand, RequestFactDeletionResult> requestDeletionCommand,
    ICommandHandler<ApproveFactDeletionCommand, FactDeletionDecisionResult> approveDeletionCommand,
    ICommandHandler<RejectFactDeletionCommand, FactDeletionDecisionResult> rejectDeletionCommand,
    IQueryHandler<GetDeletionRequestsQuery, DeletionRequestQueueSnapshot> deletionRequestsQuery,
    IQueryHandler<GetFactByIdQuery, FactDetails?> factByIdQuery,
    IWorkspaceTabService workspaceTabs) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tabId, string? status, CancellationToken cancellationToken)
    {
        workspaceTabs.OpenDeletionRequests(tabId);
        var snapshot = await deletionRequestsQuery.Handle(
            new GetDeletionRequestsQuery(status),
            cancellationToken);
        return View(snapshot);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanRequestDeletion)]
    public async Task<IActionResult> Create(int factId, string? tabId, CancellationToken cancellationToken)
    {
        var fact = await factByIdQuery.Handle(new GetFactByIdQuery(factId), cancellationToken);
        if (fact is null)
        {
            return NotFound();
        }

        if (fact.HasPendingDeletionRequest)
        {
            TempData["ErrorMessage"] = "A deletion request is already pending for this fact.";
            return RedirectToAction(nameof(Index));
        }

        var tab = workspaceTabs.OpenDeletionRequest(factId, tabId);
        return View(new RequestFactDeletionInputModel
        {
            FactId = factId,
            TabId = tab.Id,
            FactContent = fact.Content
        });
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanRequestDeletion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        RequestFactDeletionInputModel model,
        CancellationToken cancellationToken)
    {
        workspaceTabs.OpenDeletionRequest(model.FactId, model.TabId);
        model.Reason = model.Reason?.Trim() ?? string.Empty;
        if (model.Reason.Length < 10)
        {
            ModelState.AddModelError(nameof(model.Reason), "Reason must contain at least 10 non-whitespace characters.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await requestDeletionCommand.Handle(
            new RequestFactDeletionCommand(model.FactId, model.Reason),
            cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(model.Reason), result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        workspaceTabs.Close(model.TabId);
        var requestsTab = workspaceTabs.OpenDeletionRequests();
        return RedirectToAction(nameof(Index), new { tabId = requestsTab.Id });
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanDecideDeletion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        DeletionDecisionInputModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Approval note is invalid.";
            return RedirectToAction(nameof(Index), new { tabId = model.TabId });
        }

        var result = await approveDeletionCommand.Handle(
            new ApproveFactDeletionCommand(model.RequestId, model.Note),
            cancellationToken);
        TempData[result.Outcome == FactDeletionDecisionOutcome.ApprovedWithProjectionWarning
            ? "ErrorMessage"
            : result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index), new { tabId = model.TabId });
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanDecideDeletion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        DeletionDecisionInputModel model,
        CancellationToken cancellationToken)
    {
        model.Note = model.Note?.Trim();
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Note) || model.Note.Length < 10)
        {
            TempData["ErrorMessage"] = "A rejection note with at least 10 characters is required.";
            return RedirectToAction(nameof(Index), new { tabId = model.TabId });
        }

        var result = await rejectDeletionCommand.Handle(
            new RejectFactDeletionCommand(model.RequestId, model.Note),
            cancellationToken);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index), new { tabId = model.TabId });
    }
}
