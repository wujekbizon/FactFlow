using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Application.Security;

namespace FactFlow.Application.CatFacts.Commands.DecideFactDeletion;

public sealed class RejectFactDeletionCommandHandler(
    IFactDeletionRequestRepository deletionRequestRepository,
    ICurrentUserContext currentUser)
    : ICommandHandler<RejectFactDeletionCommand, FactDeletionDecisionResult>
{
    public async Task<FactDeletionDecisionResult> Handle(
        RejectFactDeletionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(AppRoles.Supervisor))
        {
            return new(FactDeletionDecisionOutcome.Forbidden, "Only supervisors can reject deletion requests.");
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

        try
        {
            request.Reject(currentUser.UserName, command.Note, DateTimeOffset.UtcNow);
            await deletionRequestRepository.SaveChangesAsync(cancellationToken);
            return new(FactDeletionDecisionOutcome.Rejected, "Deletion request rejected. The fact remains active.");
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
    }
}
