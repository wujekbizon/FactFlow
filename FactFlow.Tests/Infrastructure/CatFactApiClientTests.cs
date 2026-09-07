using System.Net;
using System.Text;
using FactFlow.Infrastructure.CatFacts;
using Microsoft.Extensions.Options;

namespace FactFlow.Tests.Infrastructure;

public sealed class CatFactApiClientTests
{
    [Fact]
    public async Task GetFactAsync_DeserializesExpectedContract()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "{\"fact\":\"A test fact.\",\"length\":12}"));
        var client = new CatFactApiClient(
            httpClient,
            Options.Create(new CatFactApiOptions { Endpoint = "https://example.test/fact" }));

        var result = await client.GetFactAsync();

        Assert.Equal("A test fact.", result.Fact);
        Assert.Equal(12, result.Length);
    }

    [Fact]
    public async Task GetFactAsync_NonSuccessStatus_Throws()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(
            HttpStatusCode.ServiceUnavailable,
            "{}"));
        var client = new CatFactApiClient(
            httpClient,
            Options.Create(new CatFactApiOptions { Endpoint = "https://example.test/fact" }));

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetFactAsync());
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
