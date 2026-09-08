namespace FactFlow.Application.CatFacts.Queries.GetDeletionRequests;

public sealed record DeletionRequestQueueSnapshot(
    bool IsSupervisor,
    int PendingCount,
    int ApprovedCount,
    int RejectedCount,
    int ExpiredCount,
    string? SelectedStatus,
    IReadOnlyList<DeletionRequestItem> Items);
