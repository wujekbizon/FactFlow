using FactFlow.Application.Abstractions;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Tests.TestDoubles;

internal sealed class InMemoryFactRepository(IEnumerable<FactRecord>? facts = null) : IFactRepository
{
    public List<FactRecord> Facts { get; } = facts?.ToList() ?? [];
    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<FactRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FactRecord>>(Facts.Where(fact => !fact.IsDeleted).ToArray());

    public Task<FactRecord?> FindAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default) =>
        Task.FromResult(Facts.FirstOrDefault(fact => fact.Id == id && !fact.IsDeleted));

    public Task<HashSet<int>> GetJournalSequencesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Facts.Select(fact => fact.JournalSequence).ToHashSet());

    public Task<int> GetNextJournalSequenceAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Facts.Count == 0 ? 1 : Facts.Max(fact => fact.JournalSequence) + 1);

    public Task AddAsync(FactRecord fact, CancellationToken cancellationToken = default)
    {
        Facts.Add(fact);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
