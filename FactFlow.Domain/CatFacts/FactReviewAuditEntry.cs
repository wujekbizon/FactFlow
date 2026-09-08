namespace FactFlow.Domain.CatFacts;

public sealed class FactReviewAuditEntry
{
    private FactReviewAuditEntry() { }

    private FactReviewAuditEntry(
        int factId,
        FactReviewStatus fromStatus,
        FactReviewStatus toStatus,
        string reviewedBy,
        string? note,
        DateTimeOffset occurredAtUtc)
    {
        FactId = factId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ReviewedBy = reviewedBy;
        Note = note;
        OccurredAtUtc = occurredAtUtc;
    }

    public int Id { get; private set; }
    public int FactId { get; private set; }
    public FactReviewStatus FromStatus { get; private set; }
    public FactReviewStatus ToStatus { get; private set; }
    public string ReviewedBy { get; private set; } = string.Empty;
    public string? Note { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static FactReviewAuditEntry Create(
        int factId,
        FactReviewStatus fromStatus,
        FactReviewStatus toStatus,
        string reviewedBy,
        string? note,
        DateTimeOffset occurredAtUtc)
    {
        if (factId < 1) throw new ArgumentOutOfRangeException(nameof(factId));
        if (!Enum.IsDefined(fromStatus)) throw new ArgumentOutOfRangeException(nameof(fromStatus));
        if (!Enum.IsDefined(toStatus)) throw new ArgumentOutOfRangeException(nameof(toStatus));

        var normalizedReviewer = reviewedBy.Trim();
        if (string.IsNullOrWhiteSpace(normalizedReviewer))
        {
            throw new ArgumentException("Reviewer is required.", nameof(reviewedBy));
        }

        return new FactReviewAuditEntry(
            factId,
            fromStatus,
            toStatus,
            normalizedReviewer,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            occurredAtUtc);
    }
}
