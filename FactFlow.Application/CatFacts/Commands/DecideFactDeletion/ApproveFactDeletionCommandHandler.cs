using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Application.Security;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.DecideFactDeletion;

public sealed class ApproveFactDeletionCommandHandler(
    IFactRepository factRepository,
    IFactDeletionRequestRepository deletionRequestRepository,
    IFactFileSynchronizer factFileSynchronizer,
    ICurrentUserContext currentUser)
    : ICommandHandler<ApproveFactDeletionCommand, FactDeletionDecisionResult>
{
    public async Task<FactDeletionDecisionResult> Handle(
        ApproveFactDeletionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(AppRoles.Supervisor))
        {
            return new(FactDeletionDecisionOutcome.Forbidden, "Only supervisors can approve deletion requests.");
        }

        var request = await deletionRequestRepository.FindAsync(
            command.RequestId,
            trackChanges: true,
            cancellationToken);
        if (request is null)
        {
            return new(FactDeletionDecisionOutcome.NotFound, "Deletion request not found.");
        }

        if (!request.IsPending)
        {
            return new(FactDeletionDecisionOutcome.AlreadyDecided, "This deletion request has already been decided.");
        }

        var fact = await factRepository.FindAsync(
            request.FactId,
            trackChanges: true,
            cancellationToken);
        if (fact is null || fact.IsDeleted)
        {
            try
            {
                request.Expire(currentUser.UserName, "The fact is no longer active.", DateTimeOffset.UtcNow);
                await SaveDecisionAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                return new(FactDeletionDecisionOutcome.Conflict, "Another user decided this request first. Refresh the queue.");
            }
            catch (InvalidOperationException exception)
            {
                return new(FactDeletionDecisionOutcome.Forbidden, exception.Message);
            }
            return new(FactDeletionDecisionOutcome.Expired, "The request expired because the fact is no longer active.");
        }

        if (!fact.RowVersion.AsSpan().SequenceEqual(request.TargetFactRowVersion))
        {
            try
            {
                request.Expire(currentUser.UserName, "The fact changed after this request was created.", DateTimeOffset.UtcNow);
                await SaveDecisionAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                return new(FactDeletionDecisionOutcome.Conflict, "Another user decided this request first. Refresh the queue.");
            }
            catch (InvalidOperationException exception)
            {
                return new(FactDeletionDecisionOutcome.Forbidden, exception.Message);
            }
            return new(FactDeletionDecisionOutcome.Expired, "The request expired because the fact changed.");
        }

        try
        {
            var decidedAtUtc = DateTimeOffset.UtcNow;
            request.Approve(currentUser.UserName, command.Note, decidedAtUtc);
            fact.SoftDelete(decidedAtUtc);
            await SaveDecisionAsync(cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return new(FactDeletionDecisionOutcome.InvalidInput, exception.Message);
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("requester", StringComparison.OrdinalIgnoreCase))
        {
            return new(FactDeletionDecisionOutcome.Forbidden, exception.Message);
        }
        catch (ConcurrencyConflictException)
        {
            return new(FactDeletionDecisionOutcome.Conflict, "Another user decided this request first. Refresh the queue.");
        }

        try
        {
            await factFileSynchronizer.SynchronizeFromDatabaseAsync(cancellationToken);
            return new(FactDeletionDecisionOutcome.Approved, "Deletion approved. The fact and TXT projection were updated.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(
                FactDeletionDecisionOutcome.ApprovedWithProjectionWarning,
                "Deletion approved in SQL, but the TXT projection could not be updated. It will be repaired on the next startup.");
        }
    }

    private Task SaveDecisionAsync(CancellationToken cancellationToken) =>
        deletionRequestRepository.SaveChangesAsync(cancellationToken);
}
