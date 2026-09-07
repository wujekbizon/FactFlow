using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactById;

public sealed record GetFactByIdQuery(int Id) : IQuery<FactDetails?>;
