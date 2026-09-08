using System.Text;
using FactFlow.Application.CatFacts.Commands.AddManualFact;
using FactFlow.Application.CatFacts.Commands.FetchCatFact;
using FactFlow.Application.CatFacts.Commands.UpdateFact;
using FactFlow.Application.CatFacts.Commands.ReviewFact;
using FactFlow.Application.CatFacts.Queries.GetFactById;
using FactFlow.Application.CatFacts.Queries.GetFacts;
using FactFlow.Application.CatFacts.Queries.GetFactHistory;
using FactFlow.Application.CatFacts.Queries.GetFactJournal;
using FactFlow.Application.CatFacts.Queries.GetReviewQueue;
using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;
using FactFlow.Web.Models;
using FactFlow.Web.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactFlow.Web.Controllers;

[Authorize]
public sealed class FactsController(
    ICommandHandler<FetchCatFactCommand, FetchCatFactResult> fetchFactCommand,
    ICommandHandler<AddManualFactCommand, FactRecord> addManualFactCommand,
    ICommandHandler<UpdateFactCommand, bool> updateFactCommand,
    ICommandHandler<ReviewFactCommand, bool> reviewFactCommand,
    IQueryHandler<GetFactsQuery, IReadOnlyList<FactListItem>> factsQuery,
    IQueryHandler<GetFactByIdQuery, FactDetails?> factByIdQuery,
    IQueryHandler<GetFactHistoryQuery, FactHistorySnapshot> historyQuery,
    IQueryHandler<GetFactJournalQuery, FactJournalFile?> journalQuery,
    IQueryHandler<GetReviewQueueQuery, ReviewQueueSnapshot> reviewQueueQuery,
    IWorkspaceTabService workspaceTabs,
    IWebHostEnvironment environment,
    ILogger<FactsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tabId, CancellationToken cancellationToken)
    {
        workspaceTabs.OpenFacts(tabId);
        var facts = await factsQuery.Handle(new GetFactsQuery(), cancellationToken);
        return View(facts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fetch(CancellationToken cancellationToken)
    {
        try
        {
            var result = await fetchFactCommand.Handle(new FetchCatFactCommand(), cancellationToken);
            TempData["SuccessMessage"] = $"Response received and appended in {result.DurationMilliseconds} ms.";
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException)
        {
            logger.LogWarning(exception, "Cat Fact intake failed");
            TempData["ErrorMessage"] = environment.IsDevelopment()
                ? $"The response could not be received or saved: {exception.Message}"
                : "The response could not be received or saved. Check the endpoint and journal path.";
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Create(string? tabId = null, bool saved = false)
    {
        var tab = workspaceTabs.OpenNewFact(tabId);
        ViewData["ClearDraft"] = saved;
        return View(new ManualFactInputModel { TabId = tab.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ManualFactInputModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            workspaceTabs.OpenNewFact(model.TabId);
            return View(model);
        }

        var tab = workspaceTabs.OpenNewFact(model.TabId);
        var fact = await addManualFactCommand.Handle(new AddManualFactCommand(model.Fact), cancellationToken);
        TempData["SuccessMessage"] = $"Manual fact saved with calculated length {fact.Length}.";
        var next = workspaceTabs.Close(tab.Id);
        return RedirectToAction(next.Action, next.Controller,
            next.Id == "dashboard" ? null : new { id = next.EntityId, tabId = next.Id });
    }

    [HttpGet]
    public async Task<IActionResult> History(string? tabId, CancellationToken cancellationToken)
    {
        workspaceTabs.OpenHistory(tabId);
        var history = await historyQuery.Handle(new GetFactHistoryQuery(), cancellationToken);
        return View(history);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? tabId, CancellationToken cancellationToken)
    {
        var fact = await factByIdQuery.Handle(new GetFactByIdQuery(id), cancellationToken);
        if (fact is null)
        {
            return NotFound();
        }

        var tab = workspaceTabs.OpenEditFact(id, tabId);
        return View(new EditFactInputModel
        {
            Id = fact.Id,
            TabId = tab.Id,
            Content = fact.Content,
            Source = fact.Source,
            JournalSequence = fact.JournalSequence,
            ReviewStatus = Enum.Parse<FactReviewStatus>(fact.ReviewStatus),
            ReviewedBy = fact.ReviewedBy,
            ReviewNote = fact.ReviewNote,
            ReviewedAtUtc = fact.ReviewedAtUtc,
            HasPendingDeletionRequest = fact.HasPendingDeletionRequest,
            ReviewHistory = fact.ReviewHistory
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditFactInputModel model, CancellationToken cancellationToken)
    {
        workspaceTabs.OpenEditFact(model.Id, model.TabId);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await updateFactCommand.Handle(
            new UpdateFactCommand(model.Id, model.Content),
            cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = $"Fact #{model.Id} updated.";
        return RedirectToAction(nameof(Edit), new { id = model.Id, tabId = model.TabId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewFactInputModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Review status or note is invalid.";
            return RedirectToAction(nameof(Edit), new { id = model.Id, tabId = model.TabId });
        }

        var reviewed = await reviewFactCommand.Handle(
            new ReviewFactCommand(model.Id, model.Status, model.Note),
            cancellationToken);
        if (!reviewed)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = $"Fact #{model.Id} marked as {model.Status}.";
        return RedirectToAction(nameof(Edit), new { id = model.Id, tabId = model.TabId });
    }

    [HttpGet]
    public async Task<IActionResult> ReviewQueue(
        string? tabId,
        string? status,
        string? source,
        string? reviewer,
        CancellationToken cancellationToken)
    {
        workspaceTabs.OpenReviewQueue(tabId);
        var queue = await reviewQueueQuery.Handle(
            new GetReviewQueueQuery(status, source, reviewer),
            cancellationToken);
        return View(queue);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadJournal(CancellationToken cancellationToken)
    {
        var journal = await journalQuery.Handle(new GetFactJournalQuery(), cancellationToken);
        return journal is null
            ? NotFound("The journal will be created after the first successful request.")
            : File(Encoding.UTF8.GetBytes(journal.Content), "text/plain; charset=utf-8", journal.FileName);
    }
}
