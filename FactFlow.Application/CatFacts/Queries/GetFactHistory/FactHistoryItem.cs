namespace FactFlow.Application.CatFacts.Queries.GetFactHistory;

public sealed record FactHistoryItem(
    int Sequence,
    string Fact,
    int DeclaredLength,
    int CalculatedLength,
    bool HasValidLength,
    bool IsDuplicate,
    string Sha256,
    string RawJson);
