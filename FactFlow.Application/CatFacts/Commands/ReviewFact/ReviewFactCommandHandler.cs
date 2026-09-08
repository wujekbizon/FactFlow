using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;
using FactFlow.Application.Security;

namespace FactFlow.Application.CatFacts.Commands.ReviewFact;

public sealed class ReviewFactCommandHandler(
    IFactRepository factRepository,
    ICurrentUserContext currentUser)
    : ICommandHandler<ReviewFactCommand, bool>
{
    public async Task<bool> Handle(ReviewFactCommand command, CancellationToken cancellationToken)
    {
        var fact = await factRepository.FindAsync(command.Id, trackChanges: true, cancellationToken);
        if (fact is null)
        {
            return false;
        }

        if (!currentUser.IsAuthenticated)
        {
            return false;
        }

        var occurredAtUtc = DateTimeOffset.UtcNow;
        var previousStatus = fact.ReviewStatus;
        fact.Review(command.Status, currentUser.UserName, command.Note, occurredAtUtc);
        await factRepository.AddReviewAuditAsync(
            FactReviewAuditEntry.Create(
                fact.Id,
                previousStatus,
                command.Status,
                currentUser.UserName,
                command.Note,
                occurredAtUtc),
            cancellationToken);
        await factRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
