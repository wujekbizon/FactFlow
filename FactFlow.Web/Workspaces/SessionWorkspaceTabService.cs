using System.Text.Json;

namespace FactFlow.Web.Workspaces;

public sealed class SessionWorkspaceTabService(IHttpContextAccessor httpContextAccessor)
    : IWorkspaceTabService
{
    private const string TabsSessionKey = "FactFlow.Workspaces";
    private const string ActiveTabSessionKey = "FactFlow.ActiveWorkspace";
    private const string DashboardId = "dashboard";
    private const string NewFactKind = "NewFact";
    private const string HistoryKind = "History";
    private static readonly WorkspaceTab Dashboard = new(
        DashboardId, "Dashboard", "Dashboard", "Home", "Index", false);

    private ISession Session => httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No active HTTP session.");

    public string ActiveTabId => Session.GetString(ActiveTabSessionKey) ?? DashboardId;

    public IReadOnlyList<WorkspaceTab> GetOpenTabs() => ReadTabs();

    public WorkspaceTab ActivateDashboard()
    {
        EnsureDashboard();
        SetActive(DashboardId);
        return Dashboard;
    }

    public WorkspaceTab OpenNewFact(string? tabId = null)
    {
        var tabs = ReadTabs();
        var existing = string.IsNullOrWhiteSpace(tabId)
            ? null
            : tabs.FirstOrDefault(tab => tab.Id == tabId && tab.Kind == NewFactKind);

        if (existing is not null)
        {
            SetActive(existing.Id);
            return existing;
        }

        var number = tabs
            .Where(tab => tab.Kind == NewFactKind)
            .Select(tab => tab.Title == "New fact"
                ? 1
                : int.TryParse(tab.Title["New fact".Length..].Trim(), out var parsed) ? parsed : 1)
            .DefaultIfEmpty(0)
            .Max() + 1;
        var tab = new WorkspaceTab(
            $"new-{Guid.NewGuid():N}",
            NewFactKind,
            number == 1 ? "New fact" : $"New fact {number}",
            "Facts",
            "Create",
            true);
        tabs.Add(tab);
        SaveTabs(tabs);
        SetActive(tab.Id);
        return tab;
    }

    public WorkspaceTab OpenHistory(string? tabId = null)
    {
        var tabs = ReadTabs();
        var existing = tabs.FirstOrDefault(tab =>
            tab.Kind == HistoryKind && (string.IsNullOrWhiteSpace(tabId) || tab.Id == tabId));

        if (existing is null)
        {
            existing = new WorkspaceTab("history", HistoryKind, "Response history", "Facts", "History", true);
            tabs.Add(existing);
            SaveTabs(tabs);
        }

        SetActive(existing.Id);
        return existing;
    }

    public WorkspaceTab Close(string tabId)
    {
        var tabs = ReadTabs();
        if (tabId == DashboardId)
        {
            return ActivateDashboard();
        }

        var removedIndex = tabs.FindIndex(tab => tab.Id == tabId);
        if (removedIndex >= 0)
        {
            tabs.RemoveAt(removedIndex);
            RenumberNewFactTabs(tabs);
            SaveTabs(tabs);
        }

        if (ActiveTabId != tabId)
        {
            return tabs.FirstOrDefault(tab => tab.Id == ActiveTabId) ?? Dashboard;
        }

        var nextIndex = Math.Clamp(removedIndex - 1, 0, tabs.Count - 1);
        var next = tabs.Count == 0 ? Dashboard : tabs[nextIndex];
        SetActive(next.Id);
        return next;
    }

    private List<WorkspaceTab> ReadTabs()
    {
        var json = Session.GetString(TabsSessionKey);
        var tabs = string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<WorkspaceTab>>(json) ?? [];

        if (tabs.All(tab => tab.Id != DashboardId))
        {
            tabs.Insert(0, Dashboard);
            SaveTabs(tabs);
        }

        return tabs;
    }

    private void EnsureDashboard() => _ = ReadTabs();

    private void SaveTabs(List<WorkspaceTab> tabs) =>
        Session.SetString(TabsSessionKey, JsonSerializer.Serialize(tabs));

    private void SetActive(string tabId) => Session.SetString(ActiveTabSessionKey, tabId);

    private static void RenumberNewFactTabs(List<WorkspaceTab> tabs)
    {
        var number = 0;
        for (var index = 0; index < tabs.Count; index++)
        {
            if (tabs[index].Kind != NewFactKind)
            {
                continue;
            }

            number++;
            tabs[index] = tabs[index] with { Title = number == 1 ? "New fact" : $"New fact {number}" };
        }
    }
}
