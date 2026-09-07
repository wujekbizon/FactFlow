namespace FactFlow.Domain.CatFacts;

public sealed class FactRecord
{
    private FactRecord() { }

    private FactRecord(CatFact fact, FactSource source, int journalSequence, DateTimeOffset createdAtUtc)
    {
        Content = fact.Fact;
        Length = fact.Length;
        Source = source;
        JournalSequence = journalSequence;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public int Id { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int Length { get; private set; }
    public FactSource Source { get; private set; }
    public int JournalSequence { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public static FactRecord Create(CatFact fact, FactSource source, int journalSequence, DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(fact);
        if (!fact.IsValid) throw new ArgumentException("A valid fact is required.", nameof(fact));
        if (journalSequence < 1) throw new ArgumentOutOfRangeException(nameof(journalSequence));

        return new FactRecord(fact, source, journalSequence, createdAtUtc);
    }

    public void UpdateContent(string content, DateTimeOffset updatedAtUtc)
    {
        var normalized = content.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Fact content is required.", nameof(content));
        }

        Content = normalized;
        Length = normalized.Length;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTimeOffset deletedAtUtc)
    {
        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        UpdatedAtUtc = deletedAtUtc;
    }
}
