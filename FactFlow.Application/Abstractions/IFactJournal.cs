using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.Abstractions;

public interface IFactJournal
{
    string FilePath { get; }

    Task<int> AppendAsync(CatFact fact, CancellationToken cancellationToken = default);

    Task ReplaceAllAsync(IReadOnlyList<CatFact> facts, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default);

    Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default);
}
