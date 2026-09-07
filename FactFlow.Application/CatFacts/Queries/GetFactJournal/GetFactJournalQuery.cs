using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactJournal;

public sealed record GetFactJournalQuery : IQuery<FactJournalFile?>;
