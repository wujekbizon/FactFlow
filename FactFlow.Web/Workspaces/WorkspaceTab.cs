namespace FactFlow.Web.Workspaces;

public sealed record WorkspaceTab(
    string Id,
    string Kind,
    string Title,
    string Controller,
    string Action,
    bool IsClosable);
