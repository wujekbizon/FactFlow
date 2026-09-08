using FactFlow.Application.Abstractions;
using FactFlow.Application.Security;
using FactFlow.Domain.CatFacts;
using Microsoft.EntityFrameworkCore;

namespace FactFlow.Infrastructure.Data;

public sealed class FactRepository(FactFlowDbContext dbContext) : IFactRepository
{
    public async Task<IReadOnlyList<FactRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Facts
            .AsNoTracking()
            .Where(fact => !fact.IsDeleted)
            .OrderByDescending(fact => fact.Id)
            .ToListAsync(cancellationToken);

    public Task<FactRecord?> FindAsync(
        int id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        var query = (trackChanges ? dbContext.Facts : dbContext.Facts.AsNoTracking())
            .Where(fact => !fact.IsDeleted);
        return query.FirstOrDefaultAsync(fact => fact.Id == id, cancellationToken);
    }

    public async Task<HashSet<int>> GetJournalSequencesAsync(CancellationToken cancellationToken = default) =>
        (await dbContext.Facts
            .AsNoTracking()
            .Select(fact => fact.JournalSequence)
            .ToListAsync(cancellationToken))
        .ToHashSet();

    public async Task<int> GetNextJournalSequenceAsync(CancellationToken cancellationToken = default)
    {
        var maximum = await dbContext.Facts
            .Select(fact => (int?)fact.JournalSequence)
            .MaxAsync(cancellationToken) ?? 0;
        return maximum + 1;
    }

    public async Task<HashSet<int>> GetPendingDeletionFactIdsAsync(CancellationToken cancellationToken = default) =>
        (await dbContext.FactDeletionRequests
            .AsNoTracking()
            .Where(request => request.Status == FactDeletionRequestStatus.Pending)
            .Select(request => request.FactId)
            .ToListAsync(cancellationToken))
        .ToHashSet();

    public async Task AddAsync(FactRecord fact, CancellationToken cancellationToken = default) =>
        await dbContext.Facts.AddAsync(fact, cancellationToken);

    public async Task AddReviewAuditAsync(
        FactReviewAuditEntry entry,
        CancellationToken cancellationToken = default) =>
        await dbContext.FactReviewAudit.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<FactReviewAuditEntry>> ListReviewAuditAsync(
        int factId,
        CancellationToken cancellationToken = default) =>
        await dbContext.FactReviewAudit
            .AsNoTracking()
            .Where(entry => entry.FactId == factId)
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ToListAsync(cancellationToken);

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
    }
}
