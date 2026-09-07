using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactHistory;

public sealed record GetFactHistoryQuery : IQuery<FactHistorySnapshot>;
