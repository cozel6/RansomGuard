using Microsoft.EntityFrameworkCore;
using RansomGuard.API.Data.Entities;

namespace RansomGuard.API.Data;

public class RansomGuardDbContext : DbContext
{
    public RansomGuardDbContext(DbContextOptions<RansomGuardDbContext> options)
        : base(options)
    {
    }

    public DbSet<AnalysisResultEntity> AnalysisResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalysisResultEntity>(entity =>
        {
            entity.ToTable("AnalysisResults");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Filename).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Verdict).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SuspiciousAPIs).HasMaxLength(4000);

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.FileHash);
        });
    }
}
