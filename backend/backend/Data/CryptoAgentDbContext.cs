using CryptoAgent.Api.Models;
using CryptoAgent.Api.Backtesting.Entities;
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
    public DbSet<DecisionLogEntity> DecisionLogs => Set<DecisionLogEntity>();
    public DbSet<Entities.HourlyCandleEntity> HourlyCandles => Set<Entities.HourlyCandleEntity>();
    public DbSet<Entities.HourlyFeatureEntity> HourlyFeatures => Set<Entities.HourlyFeatureEntity>();
    public DbSet<Entities.RegimeStateEntity> RegimeStates => Set<Entities.RegimeStateEntity>();
    public DbSet<Entities.StrategySignalEntity> StrategySignals => Set<Entities.StrategySignalEntity>();
    public DbSet<Entities.ExogenousItemEntity> ExogenousItems => Set<Entities.ExogenousItemEntity>();
    public DbSet<Entities.ExogenousClassificationEntity> ExogenousClassifications => Set<Entities.ExogenousClassificationEntity>();
    public DbSet<Entities.NarrativeEntity> Narratives => Set<Entities.NarrativeEntity>();
    public DbSet<Entities.NarrativeItemEntity> NarrativeItems => Set<Entities.NarrativeItemEntity>();
    public DbSet<Entities.DecisionInputsExogenousEntity> DecisionInputsExogenous => Set<Entities.DecisionInputsExogenousEntity>();
    public DbSet<BacktestRunEntity> BacktestRuns => Set<BacktestRunEntity>();
    public DbSet<BacktestStepEntity> BacktestSteps => Set<BacktestStepEntity>();
    public DbSet<BacktestTradeEntity> BacktestTrades => Set<BacktestTradeEntity>();
    public DbSet<BacktestMetricEntity> BacktestMetrics => Set<BacktestMetricEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PortfolioEntity>(entity =>
        {
            entity.ToTable("Portfolios");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Id).IsUnique();
            entity.Property(e => e.BtcCostBasisGbp).HasDefaultValue(0);
            entity.Property(e => e.EthCostBasisGbp).HasDefaultValue(0);
        });

        modelBuilder.Entity<TradeEntity>(entity =>
        {
            entity.ToTable("Trades");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Asset).IsRequired();
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.AssetAmount).HasDefaultValue(0);
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

        modelBuilder.Entity<DecisionLogEntity>(entity =>
        {
            entity.ToTable("DecisionLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BtcValueGbp).HasDefaultValue(0);
            entity.Property(e => e.EthValueGbp).HasDefaultValue(0);
            entity.Property(e => e.TotalValueGbp).HasDefaultValue(0);
            entity.Property(e => e.BtcUnrealisedPnlGbp).HasDefaultValue(0);
            entity.Property(e => e.EthUnrealisedPnlGbp).HasDefaultValue(0);
            entity.Property(e => e.BtcCostBasisGbp).HasDefaultValue(0);
            entity.Property(e => e.EthCostBasisGbp).HasDefaultValue(0);
            entity.Property(e => e.ExogenousTraceJson).HasDefaultValue("{}");
            entity.Property(e => e.ExogenousSummary).HasDefaultValue(string.Empty);
        });

        modelBuilder.Entity<Entities.HourlyCandleEntity>(entity =>
        {
            entity.ToTable("HourlyCandles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Asset, e.HourUtc }).IsUnique();
            entity.Property(e => e.Asset).IsRequired();
            entity.Property(e => e.Source).HasDefaultValue("CoinGecko");
        });

        modelBuilder.Entity<Entities.HourlyFeatureEntity>(entity =>
        {
            entity.ToTable("HourlyFeatures");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Asset, e.HourUtc }).IsUnique();
            entity.Property(e => e.Asset).IsRequired();
        });

        modelBuilder.Entity<Entities.RegimeStateEntity>(entity =>
        {
            entity.ToTable("RegimeStates");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Asset, e.HourUtc }).IsUnique();
            entity.Property(e => e.Asset).IsRequired();
        });

        modelBuilder.Entity<Entities.StrategySignalEntity>(entity =>
        {
            entity.ToTable("StrategySignals");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Asset, e.HourUtc });
            entity.HasIndex(e => new { e.StrategyName, e.HourUtc });
            entity.Property(e => e.Asset).IsRequired();
            entity.Property(e => e.StrategyName).IsRequired();
        });

        modelBuilder.Entity<Entities.ExogenousItemEntity>(entity =>
        {
            entity.ToTable("ExogenousItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasIndex(e => e.PublishedAt);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.SourceId).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.ContentHash).IsRequired();
        });

        modelBuilder.Entity<Entities.ExogenousClassificationEntity>(entity =>
        {
            entity.ToTable("ExogenousClassifications");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.ThemeRelevance);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.ItemId).IsRequired();
            entity.Property(e => e.ThemeRelevance).IsRequired();
            entity.Property(e => e.ImpactHorizon).IsRequired();
            entity.Property(e => e.DirectionalBias).IsRequired();
        });

        modelBuilder.Entity<Entities.NarrativeEntity>(entity =>
        {
            entity.ToTable("Narratives");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Theme);
            entity.HasIndex(e => e.LastUpdatedAt);
            entity.Property(e => e.Theme).IsRequired();
            entity.Property(e => e.Label).IsRequired();
        });

        modelBuilder.Entity<Entities.NarrativeItemEntity>(entity =>
        {
            entity.ToTable("NarrativeItems");
            entity.HasKey(e => new { e.NarrativeId, e.ItemId });
            entity.HasIndex(e => e.NarrativeId);
            entity.HasIndex(e => e.ItemId);
        });

        modelBuilder.Entity<Entities.DecisionInputsExogenousEntity>(entity =>
        {
            entity.ToTable("DecisionInputsExogenous");
            entity.HasKey(e => e.TimestampUtc);
            entity.HasIndex(e => e.TimestampUtc).IsUnique();
        });

        modelBuilder.Entity<BacktestRunEntity>(entity =>
        {
            entity.ToTable("BacktestRuns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mode).HasDefaultValue("PAPER");
            entity.Property(e => e.DecisionCadenceHours).HasDefaultValue(1);
            entity.Property(e => e.WarmupHours).HasDefaultValue(168);
            entity.Property(e => e.SelectorMode).HasDefaultValue("Deterministic");
        });

        modelBuilder.Entity<BacktestStepEntity>(entity =>
        {
            entity.ToTable("BacktestSteps");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BacktestRunId, e.HourUtc }).IsUnique();
        });

        modelBuilder.Entity<BacktestTradeEntity>(entity =>
        {
            entity.ToTable("BacktestTrades");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BacktestRunId);
        });

        modelBuilder.Entity<BacktestMetricEntity>(entity =>
        {
            entity.ToTable("BacktestMetrics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BacktestRunId).IsUnique();
        });
    }
}

public class PortfolioEntity
{
    public int Id { get; set; }
    public decimal CashGbp { get; set; }
    public decimal BtcAmount { get; set; }
    public decimal EthAmount { get; set; }
    public decimal BtcCostBasisGbp { get; set; }
    public decimal EthCostBasisGbp { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal HighWatermarkGbp { get; set; }
}

public class TradeEntity
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Asset { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public decimal AssetAmount { get; set; }
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
    public decimal BtcChange24hPct { get; set; }
    public decimal EthChange24hPct { get; set; }
    public decimal BtcChange7dPct { get; set; }
    public decimal EthPriceGbp { get; set; }
    public decimal EthChange7dPct { get; set; }
}

public class DecisionLogEntity
{
    public int Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string LlmAction { get; set; } = string.Empty;
    public string LlmAsset { get; set; } = string.Empty;
    public decimal LlmSizeGbp { get; set; }
    public decimal LlmConfidence { get; set; }
    public string ProviderUsed { get; set; } = string.Empty;
    public string RawModelOutput { get; set; } = string.Empty;

    public string FinalAction { get; set; } = string.Empty;
    public string FinalAsset { get; set; } = string.Empty;
    public decimal FinalSizeGbp { get; set; }
    public decimal BtcValueGbp { get; set; }
    public decimal EthValueGbp { get; set; }
    public decimal TotalValueGbp { get; set; }
    public decimal BtcUnrealisedPnlGbp { get; set; }
    public decimal EthUnrealisedPnlGbp { get; set; }
    public decimal BtcCostBasisGbp { get; set; }
    public decimal EthCostBasisGbp { get; set; }
    
    public bool Executed { get; set; }
    public string RationaleShort { get; set; } = string.Empty;
    public string RationaleDetailed { get; set; } = string.Empty;
    public string RiskReason { get; set; } = string.Empty;
    public string ExogenousTraceJson { get; set; } = "{}";
    public string ExogenousSummary { get; set; } = string.Empty;
    public string Mode { get; set; } = "PAPER";
}
