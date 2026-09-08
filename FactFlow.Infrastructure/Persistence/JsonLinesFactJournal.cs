using System.Text;
using System.Text.Json;
using FactFlow.Application.Abstractions;
using FactFlow.Domain.CatFacts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FactFlow.Infrastructure.Persistence;

public sealed class JsonLinesFactJournal : IFactJournal, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ILogger<JsonLinesFactJournal> _logger;

    public JsonLinesFactJournal(
        IOptions<FactJournalOptions> options,
        IHostEnvironment hostEnvironment,
        ILogger<JsonLinesFactJournal> logger)
    {
        _logger = logger;
        FilePath = ResolvePath(options.Value.Path, hostEnvironment.ContentRootPath);
    }

    public string FilePath { get; }

    public async Task<int> AppendAsync(CatFact fact, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fact);
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var nextSequence = File.Exists(FilePath)
                ? File.ReadLines(FilePath).Count(line => !string.IsNullOrWhiteSpace(line)) + 1
                : 1;
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(fact, JsonOptions);
            await File.AppendAllTextAsync(
                FilePath,
                json + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);

            _logger.LogInformation("Appended Cat Fact response to {JournalPath}", FilePath);
            return nextSequence;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<CatFact>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(FilePath))
            {
                return Array.Empty<CatFact>();
            }

            var lines = await File.ReadAllLinesAsync(FilePath, cancellationToken);
            var facts = new List<CatFact>(lines.Length);

            foreach (var line in lines.Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                try
                {
                    var fact = JsonSerializer.Deserialize<CatFact>(line, JsonOptions);
                    if (fact is { IsValid: true })
                    {
                        facts.Add(fact);
                    }
                }
                catch (JsonException exception)
                {
                    _logger.LogWarning(exception, "Skipped malformed journal line");
                }
            }

            return facts;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ReplaceAllAsync(
        IReadOnlyList<CatFact> facts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(facts);
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var lines = facts.Select(fact => JsonSerializer.Serialize(fact, JsonOptions));
            var content = string.Join(Environment.NewLine, lines);
            if (facts.Count > 0)
            {
                content += Environment.NewLine;
            }

            var temporaryPath = $"{FilePath}.{Guid.NewGuid():N}.tmp";
            try
            {
                await File.WriteAllTextAsync(
                    temporaryPath,
                    content,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    cancellationToken);
                File.Move(temporaryPath, FilePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }

            _logger.LogInformation("Synchronized {Count} database facts to {JournalPath}", facts.Count, FilePath);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<string?> ReadRawContentAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            return File.Exists(FilePath)
                ? await File.ReadAllTextAsync(FilePath, cancellationToken)
                : null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();

    private static string ResolvePath(string configuredPath, string contentRootPath)
    {
        var home = Environment.GetEnvironmentVariable("HOME") ?? contentRootPath;
        var expanded = Environment.ExpandEnvironmentVariables(configuredPath)
            .Replace("{HOME}", home, StringComparison.OrdinalIgnoreCase);

        return Path.IsPathRooted(expanded)
            ? Path.GetFullPath(expanded)
            : Path.GetFullPath(expanded, contentRootPath);
    }
}
