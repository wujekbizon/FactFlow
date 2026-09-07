using System.Text.Json.Serialization;

namespace FactFlow.Domain.CatFacts;

public sealed record CatFact(string Fact, int Length)
{
    [JsonIgnore]
    public bool IsValid => !string.IsNullOrWhiteSpace(Fact) && Length >= 0;
}
