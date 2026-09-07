using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.Abstractions;

public interface IFactRepository
{
    Task<IReadOnlyList<FactRecord>> ListAsync(CancellationToken cancellationToken = default);
    Task<FactRecord?> FindAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default);
    Task<HashSet<int>> GetJournalSequencesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FactRecord fact, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
