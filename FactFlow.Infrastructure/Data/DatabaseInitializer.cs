using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FactFlow.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FactFlowDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var synchronizer = scope.ServiceProvider.GetRequiredService<JournalProjectionSynchronizer>();
        await synchronizer.SynchronizeAsync(cancellationToken);
    }
}
