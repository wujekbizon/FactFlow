using System.Net.Http.Json;
using FactFlow.Application.Abstractions;
using FactFlow.Domain.CatFacts;
using Microsoft.Extensions.Options;

namespace FactFlow.Infrastructure.CatFacts;

public sealed class CatFactApiClient(
    HttpClient httpClient,
    IOptions<CatFactApiOptions> options)
    : ICatFactClient
{
    public async Task<CatFact> GetFactAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(options.Value.Endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fact = await response.Content.ReadFromJsonAsync<CatFact>(cancellationToken);
        return fact ?? throw new InvalidDataException("The Cat Fact API returned an empty response.");
    }
}
