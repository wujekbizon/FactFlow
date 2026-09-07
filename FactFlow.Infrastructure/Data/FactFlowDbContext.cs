using FactFlow.Domain.CatFacts;
using Microsoft.EntityFrameworkCore;

namespace FactFlow.Infrastructure.Data;

public sealed class FactFlowDbContext(DbContextOptions<FactFlowDbContext> options) : DbContext(options)
{
    public DbSet<FactRecord> Facts => Set<FactRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var fact = modelBuilder.Entity<FactRecord>();
        fact.ToTable("Facts");
        fact.HasKey(record => record.Id);
        fact.Property(record => record.Content).HasMaxLength(1000).IsRequired();
        fact.Property(record => record.Length).IsRequired();
        fact.Property(record => record.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        fact.Property(record => record.JournalSequence).IsRequired();
        fact.Property(record => record.CreatedAtUtc).IsRequired();
        fact.Property(record => record.UpdatedAtUtc).IsRequired();
        fact.Property(record => record.IsDeleted).HasDefaultValue(false).IsRequired();
        fact.Property(record => record.DeletedAtUtc);
        fact.HasIndex(record => record.JournalSequence).IsUnique();
        fact.HasIndex(record => record.CreatedAtUtc);
    }
}
