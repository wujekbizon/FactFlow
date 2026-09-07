using FactFlow.Domain.CatFacts;
using FactFlow.Infrastructure.Persistence;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FactFlow.Tests.Infrastructure;

public sealed class JsonLinesFactJournalTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(
        Path.GetTempPath(),
        "FactFlow.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task AppendAsync_WritesOneCompactJsonObjectPerLine()
    {
        var journal = CreateJournal();

        await journal.AppendAsync(new CatFact("First", 5));
        await journal.AppendAsync(new CatFact("Second", 6));

        var lines = await File.ReadAllLinesAsync(journal.FilePath);
        Assert.Equal(2, lines.Length);
        Assert.Equal("{\"fact\":\"First\",\"length\":5}", lines[0]);
        Assert.Equal("{\"fact\":\"Second\",\"length\":6}", lines[1]);
    }

    [Fact]
    public async Task ReadAllAsync_SkipsMalformedLines()
    {
        Directory.CreateDirectory(_temporaryDirectory);
        var path = Path.Combine(_temporaryDirectory, "facts.txt");
        await File.WriteAllLinesAsync(path,
        [
            "{\"fact\":\"Valid\",\"length\":5}",
            "not-json"
        ]);
        using var journal = CreateJournal();

        var facts = await journal.ReadAllAsync();

        var fact = Assert.Single(facts);
        Assert.Equal("Valid", fact.Fact);
    }

    public void Dispose()
    {
        var resolved = Path.GetFullPath(_temporaryDirectory);
        var expectedRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "FactFlow.Tests"));

        if (Directory.Exists(resolved) && resolved.StartsWith(expectedRoot, StringComparison.OrdinalIgnoreCase))
        {
            Directory.Delete(resolved, recursive: true);
        }
    }

    private JsonLinesFactJournal CreateJournal()
    {
        Directory.CreateDirectory(_temporaryDirectory);
        return new JsonLinesFactJournal(
            Options.Create(new FactJournalOptions { Path = "facts.txt" }),
            new StubHostEnvironment(_temporaryDirectory),
            NullLogger<JsonLinesFactJournal>.Instance);
    }

    private sealed class StubHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "FactFlow.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
