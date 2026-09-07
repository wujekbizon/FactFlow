using FactFlow.Application.Abstractions;
using FactFlow.Infrastructure.CatFacts;
using FactFlow.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FactFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<CatFactApiOptions>()
            .Bind(configuration.GetSection(CatFactApiOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _), "A valid Cat Fact endpoint is required.")
            .ValidateOnStart();

        services
            .AddOptions<FactJournalOptions>()
            .Bind(configuration.GetSection(FactJournalOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Path), "A journal path is required.")
            .ValidateOnStart();

        services.AddHttpClient<ICatFactClient, CatFactApiClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FactFlow/1.0");
        });

        services.AddSingleton<IFactJournal, JsonLinesFactJournal>();
        return services;
    }
}
