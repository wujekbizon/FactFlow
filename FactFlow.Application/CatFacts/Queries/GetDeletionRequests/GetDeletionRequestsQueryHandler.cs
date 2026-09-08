using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;
using FactFlow.Application.Security;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Queries.GetDeletionRequests;

public sealed class GetDeletionRequestsQueryHandler(
    IFactDeletionRequestRepository deletionRequestRepository,
    ICurrentUserContext currentUser)
    : IQueryHandler<GetDeletionRequestsQuery, DeletionRequestQueueSnapshot>
{
    public async Task<DeletionRequestQueueSnapshot> Handle(
        GetDeletionRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var isSupervisor = currentUser.IsInRole(AppRoles.Supervisor);
        var isOperator = currentUser.IsInRole(AppRoles.Operator);
        if (!currentUser.IsAuthenticated || (!isSupervisor && !isOperator))
        {
            return new(false, 0, 0, 0, 0, query.Status, []);
        }

        var requestedBy = isSupervisor ? null : currentUser.UserName;
        var status = Enum.TryParse<FactDeletionRequestStatus>(query.Status, true, out var parsedStatus)
            ? parsedStatus
            : (FactDeletionRequestStatus?)null;
        var allRequests = await deletionRequestRepository.ListAsync(requestedBy, cancellationToken: cancellationToken);
        var requests = status is null
            ? allRequests
            : allRequests.Where(request => request.Status == status.Value).ToArray();

        return new DeletionRequestQueueSnapshot(
            isSupervisor,
            allRequests.Count(request => request.Status == FactDeletionRequestStatus.Pending),
            allRequests.Count(request => request.Status == FactDeletionRequestStatus.Approved),
            allRequests.Count(request => request.Status == FactDeletionRequestStatus.Rejected),
            allRequests.Count(request => request.Status == FactDeletionRequestStatus.Expired),
            query.Status,
            requests.Select(request => new DeletionRequestItem(
                request.Id,
                request.FactId,
                request.FactContentSnapshot,
                request.Status.ToString(),
                request.RequestedBy,
                request.Reason,
                request.RequestedAtUtc,
                request.DecidedBy,
                request.DecisionNote,
                request.DecidedAtUtc)).ToArray());
    }
}
