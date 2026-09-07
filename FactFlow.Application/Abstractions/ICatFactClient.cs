using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.Abstractions;

public interface ICatFactClient
{
    Task<CatFact> GetFactAsync(CancellationToken cancellationToken = default);
}
