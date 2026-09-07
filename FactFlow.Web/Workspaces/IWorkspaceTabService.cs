namespace FactFlow.Web.Workspaces;

public interface IWorkspaceTabService
{
    string ActiveTabId { get; }

    IReadOnlyList<WorkspaceTab> GetOpenTabs();

    WorkspaceTab ActivateDashboard();

    WorkspaceTab OpenNewFact(string? tabId = null);

    WorkspaceTab OpenHistory(string? tabId = null);

    WorkspaceTab Close(string tabId);
}
