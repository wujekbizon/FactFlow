using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FactFlow.Infrastructure.Data;

public sealed class FactFlowDbContextFactory : IDesignTimeDbContextFactory<FactFlowDbContext>
{
    public FactFlowDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FactFlowDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\FactFlowLocalDb;Database=FactFlowAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
                sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null))
            .Options;

        return new FactFlowDbContext(options);
    }
}
