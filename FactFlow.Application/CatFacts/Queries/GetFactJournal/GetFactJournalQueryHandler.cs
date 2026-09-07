using FactFlow.Application.Abstractions;
using FactFlow.Application.Common.Messaging;

namespace FactFlow.Application.CatFacts.Queries.GetFactJournal;

public sealed class GetFactJournalQueryHandler(IFactJournal factJournal)
    : IQueryHandler<GetFactJournalQuery, FactJournalFile?>
{
    public async Task<FactJournalFile?> Handle(
        GetFactJournalQuery query,
        CancellationToken cancellationToken)
    {
        var content = await factJournal.ReadRawContentAsync(cancellationToken);
        return content is null ? null : new FactJournalFile(Path.GetFileName(factJournal.FilePath), content);
    }
}
