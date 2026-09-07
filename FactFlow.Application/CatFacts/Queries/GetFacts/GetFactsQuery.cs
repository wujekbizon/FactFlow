using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFacts;

public sealed record GetFactsQuery : IQuery<IReadOnlyList<FactListItem>>;
