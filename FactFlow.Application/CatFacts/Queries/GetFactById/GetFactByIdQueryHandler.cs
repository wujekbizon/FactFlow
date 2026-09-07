using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactById;

public sealed class GetFactByIdQueryHandler(IFactRepository factRepository)
    : IQueryHandler<GetFactByIdQuery, FactDetails?>
{
    public async Task<FactDetails?> Handle(GetFactByIdQuery query, CancellationToken cancellationToken)
    {
        var fact = await factRepository.FindAsync(query.Id, cancellationToken: cancellationToken);
        return fact is null
            ? null
            : new FactDetails(
                fact.Id,
                fact.Content,
                fact.Length,
                fact.Source.ToString(),
                fact.JournalSequence,
                fact.CreatedAtUtc,
                fact.UpdatedAtUtc);
    }
}
