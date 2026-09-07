namespace FactFlow.Application.CatFacts.Queries.GetDashboard;

public sealed record DashboardSnapshot(
    int TotalRequests,
    int UniqueFacts,
    double AverageLength,
    int LongestFactLength,
    string JournalPath,
    IReadOnlyList<DashboardFactItem> RecentFacts)
{
    public DashboardFactItem? LatestFact => RecentFacts.FirstOrDefault();
}
