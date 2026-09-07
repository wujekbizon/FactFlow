namespace FactFlow.Infrastructure.Persistence;

public sealed class FactJournalOptions
{
    public const string SectionName = "FactJournal";

    public string Path { get; set; } = "App_Data/catfacts.txt";
}
