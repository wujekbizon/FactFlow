using FactFlow.Application.Abstractions;
using FactFlow.Application.Security;
using FactFlow.Domain.CatFacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace FactFlow.Infrastructure.Data;

public sealed class FactDeletionRequestRepository(FactFlowDbContext dbContext)
    : IFactDeletionRequestRepository
{
    public Task<FactDeletionRequest?> FindAsync(
        int id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        var query = trackChanges
            ? dbContext.FactDeletionRequests.AsQueryable()
            : dbContext.FactDeletionRequests.AsNoTracking();
        return query.FirstOrDefaultAsync(request => request.Id == id, cancellationToken);
    }

    public Task<bool> HasPendingForFactAsync(int factId, CancellationToken cancellationToken = default) =>
        dbContext.FactDeletionRequests.AnyAsync(
            request => request.FactId == factId && request.Status == FactDeletionRequestStatus.Pending,
            cancellationToken);

    public async Task<IReadOnlyList<FactDeletionRequest>> ListAsync(
        string? requestedBy = null,
        FactDeletionRequestStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.FactDeletionRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(requestedBy))
        {
            query = query.Where(request => request.RequestedBy == requestedBy);
        }

        if (status is not null)
        {
            query = query.Where(request => request.Status == status.Value);
        }

        return await query
            .OrderByDescending(request => request.RequestedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FactDeletionRequest request, CancellationToken cancellationToken = default) =>
        await dbContext.FactDeletionRequests.AddAsync(request, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DuplicatePendingDeletionRequestException(exception);
        }
    }
}
