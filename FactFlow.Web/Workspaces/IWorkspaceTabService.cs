namespace FactFlow.Web.Workspaces;

public interface IWorkspaceTabService
{
    string ActiveTabId { get; }

    IReadOnlyList<WorkspaceTab> GetOpenTabs();

    WorkspaceTab ActivateDashboard();

    WorkspaceTab OpenFacts(string? tabId = null);

    WorkspaceTab OpenNewFact(string? tabId = null);

    WorkspaceTab OpenHistory(string? tabId = null);

    WorkspaceTab OpenEditFact(int factId, string? tabId = null);

    WorkspaceTab Close(string tabId);
}
