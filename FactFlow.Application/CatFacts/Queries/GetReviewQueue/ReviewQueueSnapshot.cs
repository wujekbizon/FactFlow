namespace FactFlow.Application.CatFacts.Queries.GetReviewQueue;

public sealed record ReviewQueueSnapshot(
    int Total,
    int NewCount,
    int ReviewedCount,
    int ApprovedCount,
    int RejectedCount,
    string? SelectedStatus,
    string? SelectedSource,
    string? SelectedReviewer,
    IReadOnlyList<string> Reviewers,
    IReadOnlyList<ReviewQueueItem> Items);
