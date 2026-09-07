namespace FactFlow.Infrastructure.CatFacts;

public sealed class CatFactApiOptions
{
    public const string SectionName = "CatFactApi";

    public string Endpoint { get; set; } = "https://catfact.ninja/fact";
}
