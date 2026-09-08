namespace FactFlow.Domain.CatFacts;

public sealed class FactDeletionRequest
{
    private FactDeletionRequest() { }

    private FactDeletionRequest(
        int factId,
        string factContentSnapshot,
        byte[] targetFactRowVersion,
        string requestedBy,
        string reason,
        DateTimeOffset requestedAtUtc)
    {
        FactId = factId;
        FactContentSnapshot = factContentSnapshot;
        TargetFactRowVersion = targetFactRowVersion;
        RequestedBy = requestedBy;
        Reason = reason;
        RequestedAtUtc = requestedAtUtc;
        Status = FactDeletionRequestStatus.Pending;
    }

    public int Id { get; private set; }
    public int FactId { get; private set; }
    public string FactContentSnapshot { get; private set; } = string.Empty;
    public byte[] TargetFactRowVersion { get; private set; } = [];
    public FactDeletionRequestStatus Status { get; private set; }
    public string RequestedBy { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public string? DecidedBy { get; private set; }
    public string? DecisionNote { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public bool IsPending => Status == FactDeletionRequestStatus.Pending;

    public static FactDeletionRequest Create(
        FactRecord fact,
        string requestedBy,
        string reason,
        DateTimeOffset requestedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(fact);
        if (fact.Id < 1) throw new ArgumentOutOfRangeException(nameof(fact));

        var normalizedRequester = NormalizeRequired(requestedBy, nameof(requestedBy), 200);
        var normalizedReason = NormalizeRequired(reason, nameof(reason), 1000);
        if (normalizedReason.Length < 10)
        {
            throw new ArgumentException("A deletion reason must contain at least 10 characters.", nameof(reason));
        }

        return new FactDeletionRequest(
            fact.Id,
            fact.Content,
            fact.RowVersion.ToArray(),
            normalizedRequester,
            normalizedReason,
            requestedAtUtc);
    }

    public void Approve(string supervisor, string? note, DateTimeOffset decidedAtUtc)
    {
        EnsurePending();
        EnsureDifferentActor(supervisor);
        Decide(FactDeletionRequestStatus.Approved, supervisor, note, decidedAtUtc);
    }

    public void Reject(string supervisor, string note, DateTimeOffset decidedAtUtc)
    {
        EnsurePending();
        EnsureDifferentActor(supervisor);
        var normalizedNote = NormalizeRequired(note, nameof(note), 1000);
        if (normalizedNote.Length < 10)
        {
            throw new ArgumentException("A rejection note must contain at least 10 characters.", nameof(note));
        }

        Decide(FactDeletionRequestStatus.Rejected, supervisor, normalizedNote, decidedAtUtc);
    }

    public void Expire(string supervisor, string note, DateTimeOffset decidedAtUtc)
    {
        EnsurePending();
        EnsureDifferentActor(supervisor);
        Decide(FactDeletionRequestStatus.Expired, supervisor, note, decidedAtUtc);
    }

    private void Decide(
        FactDeletionRequestStatus status,
        string supervisor,
        string? note,
        DateTimeOffset decidedAtUtc)
    {
        Status = status;
        DecidedBy = NormalizeRequired(supervisor, nameof(supervisor), 200);
        DecisionNote = NormalizeOptional(note, nameof(note), 1000);
        DecidedAtUtc = decidedAtUtc;
    }

    private void EnsurePending()
    {
        if (!IsPending)
        {
            throw new InvalidOperationException("Only pending deletion requests can be decided.");
        }
    }

    private void EnsureDifferentActor(string supervisor)
    {
        if (string.Equals(RequestedBy, supervisor.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The requester cannot decide their own deletion request.");
        }
    }

    private static string NormalizeRequired(string? value, string parameterName, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"A value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"A value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }
}
