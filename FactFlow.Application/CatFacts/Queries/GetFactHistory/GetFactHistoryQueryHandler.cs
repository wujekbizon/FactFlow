using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactHistory;

public sealed class GetFactHistoryQueryHandler(IFactJournal factJournal)
    : IQueryHandler<GetFactHistoryQuery, FactHistorySnapshot>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<FactHistorySnapshot> Handle(
        GetFactHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var facts = await factJournal.ReadAllAsync(cancellationToken);
        var occurrences = facts
            .GroupBy(fact => fact.Fact, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var records = facts
            .Select((fact, index) =>
            {
                var rawJson = JsonSerializer.Serialize(fact, JsonOptions);
                var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawJson)));
                return new FactHistoryItem(
                    Sequence: index + 1,
                    Fact: fact.Fact,
                    DeclaredLength: fact.Length,
                    CalculatedLength: fact.Fact.Length,
                    HasValidLength: fact.Length == fact.Fact.Length,
                    IsDuplicate: occurrences[fact.Fact] > 1,
                    Sha256: hash,
                    RawJson: rawJson);
            })
            .Reverse()
            .ToArray();

        return new FactHistorySnapshot(
            TotalRecords: records.Length,
            UniqueRecords: occurrences.Count,
            DuplicateRecords: records.Count(record => record.IsDuplicate),
            IntegrityFailures: records.Count(record => !record.HasValidLength),
            JournalPath: factJournal.FilePath,
            Records: records);
    }
}
