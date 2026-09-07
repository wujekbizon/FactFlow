using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetDashboard;

public sealed record GetDashboardQuery : IQuery<DashboardSnapshot>;
