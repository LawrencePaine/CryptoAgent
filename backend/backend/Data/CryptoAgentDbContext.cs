using CryptoAgent.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Data;

public class CryptoAgentDbContext : DbContext
{
    public CryptoAgentDbContext(DbContextOptions<CryptoAgentDbContext> options) : base(options)
    {
    }

    public DbSet<PortfolioEntity> Portfolios => Set<PortfolioEntity>();
    public DbSet<TradeEntity> Trades => Set<TradeEntity>();
    public DbSet<PerformanceSnapshotEntity> PerformanceSnapshots => Set<PerformanceSnapshotEntity>();
    public DbSet<MarketSnapshotEntity> MarketSnapshots => Set<MarketSnapshotEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PortfolioEntity>(entity =>
        {
            entity.ToTable("Portfolios");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Id).IsUnique();
        });

        modelBuilder.Entity<TradeEntity>(entity =>
        {
            entity.ToTable("Trades");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Asset).IsRequired();
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.Mode).HasDefaultValue("PAPER");
        });

        modelBuilder.Entity<PerformanceSnapshotEntity>(entity =>
        {
            entity.ToTable("PerformanceSnapshots");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<MarketSnapshotEntity>(entity =>
        {
            entity.ToTable("MarketSnapshots");
            entity.HasKey(e => e.Id);
        });
    }
}

public class PortfolioEntity
{
    public int Id { get; set; }
    public decimal CashGbp { get; set; }
    public decimal BtcAmount { get; set; }
    public decimal EthAmount { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal HighWatermarkGbp { get; set; }
}

public class TradeEntity
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Asset { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public decimal SizeGbp { get; set; }
    public decimal PriceGbp { get; set; }
    public decimal FeeGbp { get; set; }
    public string Mode { get; set; } = "PAPER";
}

public class PerformanceSnapshotEntity
{
    public int Id { get; set; }
    public DateTime DateUtc { get; set; }
    public decimal PortfolioValueGbp { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal NetDepositsGbp { get; set; }
    public decimal CumulatedAiCostGbp { get; set; }
    public decimal CumulatedFeesGbp { get; set; }
}

public class MarketSnapshotEntity
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public decimal BtcPriceGbp { get; set; }
    public decimal EthPriceGbp { get; set; }
    public decimal BtcChange24hPct { get; set; }
    public decimal EthChange24hPct { get; set; }
    public decimal BtcChange7dPct { get; set; }
    public decimal EthChange7dPct { get; set; }
}
