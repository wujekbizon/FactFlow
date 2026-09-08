using FactFlow.Application.Abstractions;
using FactFlow.Infrastructure.CatFacts;
using FactFlow.Infrastructure.Data;
using FactFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FactFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FactFlow");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'FactFlow' is required.");
        }

        services.AddDbContext<FactFlowDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));
        services.AddScoped<IFactRepository, FactRepository>();
        services.AddScoped<FactFileSynchronizer>();
        services.AddScoped<IFactFileSynchronizer>(services => services.GetRequiredService<FactFileSynchronizer>());

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
