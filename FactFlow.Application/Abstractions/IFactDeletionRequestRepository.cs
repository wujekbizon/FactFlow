using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.Abstractions;

public interface IFactDeletionRequestRepository
{
    Task<FactDeletionRequest?> FindAsync(
        int id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingForFactAsync(int factId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FactDeletionRequest>> ListAsync(
        string? requestedBy = null,
        FactDeletionRequestStatus? status = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(FactDeletionRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
