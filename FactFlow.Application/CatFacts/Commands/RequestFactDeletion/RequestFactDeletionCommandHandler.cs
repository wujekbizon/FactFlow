using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Application.Security;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.RequestFactDeletion;

public sealed class RequestFactDeletionCommandHandler(
    IFactRepository factRepository,
    IFactDeletionRequestRepository deletionRequestRepository,
    ICurrentUserContext currentUser)
    : ICommandHandler<RequestFactDeletionCommand, RequestFactDeletionResult>
{
    public async Task<RequestFactDeletionResult> Handle(
        RequestFactDeletionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(AppRoles.Operator))
        {
            return new(RequestFactDeletionOutcome.Forbidden, "Only operators can request deletion.");
        }

        var fact = await factRepository.FindAsync(command.FactId, cancellationToken: cancellationToken);
        if (fact is null)
        {
            return new(RequestFactDeletionOutcome.NotFound, "Fact not found.");
        }

        if (await deletionRequestRepository.HasPendingForFactAsync(command.FactId, cancellationToken))
        {
            return new(RequestFactDeletionOutcome.AlreadyPending, "A deletion request is already pending for this fact.");
        }

        FactDeletionRequest request;
        try
        {
            request = FactDeletionRequest.Create(
                fact,
                currentUser.UserName,
                command.Reason,
                DateTimeOffset.UtcNow);
            await deletionRequestRepository.AddAsync(request, cancellationToken);
            await deletionRequestRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return new(RequestFactDeletionOutcome.InvalidReason, exception.Message);
        }
        catch (DuplicatePendingDeletionRequestException)
        {
            return new(RequestFactDeletionOutcome.AlreadyPending, "A deletion request is already pending for this fact.");
        }

        return new(RequestFactDeletionOutcome.Created, "Deletion request sent to a supervisor.", request.Id);
    }
}
