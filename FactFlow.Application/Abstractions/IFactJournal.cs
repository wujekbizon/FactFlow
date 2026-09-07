using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.Abstractions;

public interface IFactJournal
{
    string FilePath { get; }

    Task AppendAsync(CatFact fact, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default);

    Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default);
}
