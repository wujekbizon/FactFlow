using FactFlow.Domain.CatFacts;
using Microsoft.EntityFrameworkCore;

namespace FactFlow.Infrastructure.Data;

public sealed class FactFlowDbContext(DbContextOptions<FactFlowDbContext> options) : DbContext(options)
{
    public DbSet<FactRecord> Facts => Set<FactRecord>();
    public DbSet<FactReviewAuditEntry> FactReviewAudit => Set<FactReviewAuditEntry>();
    public DbSet<FactDeletionRequest> FactDeletionRequests => Set<FactDeletionRequest>();

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
        fact.Property(record => record.ReviewStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(FactReviewStatus.New)
            .IsRequired();
        fact.Property(record => record.ReviewedBy).HasMaxLength(200);
        fact.Property(record => record.ReviewNote).HasMaxLength(2000);
        fact.Property(record => record.ReviewedAtUtc);
        fact.Property(record => record.RowVersion).IsRowVersion().IsConcurrencyToken();
        fact.HasIndex(record => record.JournalSequence).IsUnique();
        fact.HasIndex(record => record.CreatedAtUtc);

        var reviewAudit = modelBuilder.Entity<FactReviewAuditEntry>();
        reviewAudit.ToTable("FactReviewAudit");
        reviewAudit.HasKey(entry => entry.Id);
        reviewAudit.Property(entry => entry.FromStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        reviewAudit.Property(entry => entry.ToStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        reviewAudit.Property(entry => entry.ReviewedBy).HasMaxLength(200).IsRequired();
        reviewAudit.Property(entry => entry.Note).HasMaxLength(2000);
        reviewAudit.Property(entry => entry.OccurredAtUtc).IsRequired();
        reviewAudit.HasIndex(entry => new { entry.FactId, entry.OccurredAtUtc });
        reviewAudit
            .HasOne<FactRecord>()
            .WithMany()
            .HasForeignKey(entry => entry.FactId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletionRequest = modelBuilder.Entity<FactDeletionRequest>();
        deletionRequest.ToTable("FactDeletionRequests");
        deletionRequest.HasKey(request => request.Id);
        deletionRequest.Property(request => request.FactContentSnapshot).HasMaxLength(1000).IsRequired();
        deletionRequest.Property(request => request.TargetFactRowVersion).HasMaxLength(8).IsRequired();
        deletionRequest.Property(request => request.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        deletionRequest.Property(request => request.RequestedBy).HasMaxLength(200).IsRequired();
        deletionRequest.Property(request => request.Reason).HasMaxLength(1000).IsRequired();
        deletionRequest.Property(request => request.RequestedAtUtc).IsRequired();
        deletionRequest.Property(request => request.DecidedBy).HasMaxLength(200);
        deletionRequest.Property(request => request.DecisionNote).HasMaxLength(1000);
        deletionRequest.Property(request => request.DecidedAtUtc);
        deletionRequest.Property(request => request.RowVersion).IsRowVersion().IsConcurrencyToken();
        deletionRequest
            .HasOne<FactRecord>()
            .WithMany()
            .HasForeignKey(request => request.FactId)
            .OnDelete(DeleteBehavior.Restrict);
        deletionRequest
            .HasIndex(request => new { request.FactId, request.Status })
            .IsUnique()
            .HasFilter("[Status] = 'Pending'");
        deletionRequest.HasIndex(request => new { request.Status, request.RequestedAtUtc });
        deletionRequest.HasIndex(request => new { request.RequestedBy, request.RequestedAtUtc });
    }
}
