using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BacktestMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BacktestRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalValueGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetProfitGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetProfitPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxDrawdownPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    TradeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WinRatePct = table.Column<decimal>(type: "TEXT", nullable: true),
                    AvgTradePnlGbp = table.Column<decimal>(type: "TEXT", nullable: true),
                    FeesPaidGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    SlippagePaidGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    SharpeLike = table.Column<decimal>(type: "TEXT", nullable: true),
                    Baseline_Hodl_FinalValueGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Baseline_Dca_FinalValueGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Baseline_Rebalance_FinalValueGbp = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BacktestRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "PAPER"),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartHourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndHourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WarmupHours = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 168),
                    FeePct = table.Column<decimal>(type: "TEXT", nullable: false),
                    SlippagePct = table.Column<decimal>(type: "TEXT", nullable: false),
                    InitialCashGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxTradeSizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxTradesPerDay = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxBtcAllocationPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxEthAllocationPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinCashAllocationPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    DecisionCadenceHours = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    SelectorMode = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Deterministic"),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BacktestSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BacktestRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BtcCloseGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthCloseGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcRegime = table.Column<string>(type: "TEXT", nullable: false),
                    EthRegime = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    SizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Executed = table.Column<bool>(type: "INTEGER", nullable: false),
                    RiskReason = table.Column<string>(type: "TEXT", nullable: false),
                    CashGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    VaultGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalValueGbp = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestSteps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BacktestTrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BacktestRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    Side = table.Column<string>(type: "TEXT", nullable: false),
                    SizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    PriceGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    FeeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    SlippageGbp = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestTrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecisionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LlmAction = table.Column<string>(type: "TEXT", nullable: false),
                    LlmAsset = table.Column<string>(type: "TEXT", nullable: false),
                    LlmSizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    LlmConfidence = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProviderUsed = table.Column<string>(type: "TEXT", nullable: false),
                    RawModelOutput = table.Column<string>(type: "TEXT", nullable: false),
                    FinalAction = table.Column<string>(type: "TEXT", nullable: false),
                    FinalAsset = table.Column<string>(type: "TEXT", nullable: false),
                    FinalSizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Executed = table.Column<bool>(type: "INTEGER", nullable: false),
                    RationaleShort = table.Column<string>(type: "TEXT", nullable: false),
                    RationaleDetailed = table.Column<string>(type: "TEXT", nullable: false),
                    RiskReason = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HourlyCandles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OpenGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    HighGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    LowGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    CloseGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "CoinGecko")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyCandles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HourlyFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Return1h = table.Column<decimal>(type: "TEXT", nullable: false),
                    Return24h = table.Column<decimal>(type: "TEXT", nullable: false),
                    Return7d = table.Column<decimal>(type: "TEXT", nullable: false),
                    Vol24h = table.Column<decimal>(type: "TEXT", nullable: false),
                    Vol72h = table.Column<decimal>(type: "TEXT", nullable: false),
                    Sma24 = table.Column<decimal>(type: "TEXT", nullable: false),
                    Sma168 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TrendStrength = table.Column<decimal>(type: "TEXT", nullable: false),
                    Drawdown7d = table.Column<decimal>(type: "TEXT", nullable: false),
                    MomentumScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsComplete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BtcPriceGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcChange24hPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthChange24hPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcChange7dPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthPriceGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthChange7dPct = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PortfolioValueGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    VaultGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetDepositsGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    CumulatedAiCostGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    CumulatedFeesGbp = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CashGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    VaultGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    HighWatermarkGbp = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegimeStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Regime = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Confidence = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegimeStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StrategySignals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StrategyName = table.Column<string>(type: "TEXT", nullable: false),
                    SignalScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    SuggestedAction = table.Column<string>(type: "TEXT", nullable: false),
                    SuggestedSizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategySignals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Asset = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    SizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    PriceGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    FeeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "PAPER")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestMetrics_BacktestRunId",
                table: "BacktestMetrics",
                column: "BacktestRunId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BacktestSteps_BacktestRunId_HourUtc",
                table: "BacktestSteps",
                columns: new[] { "BacktestRunId", "HourUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BacktestTrades_BacktestRunId",
                table: "BacktestTrades",
                column: "BacktestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyCandles_Asset_HourUtc",
                table: "HourlyCandles",
                columns: new[] { "Asset", "HourUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyFeatures_Asset_HourUtc",
                table: "HourlyFeatures",
                columns: new[] { "Asset", "HourUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_Id",
                table: "Portfolios",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegimeStates_Asset_HourUtc",
                table: "RegimeStates",
                columns: new[] { "Asset", "HourUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StrategySignals_Asset_HourUtc",
                table: "StrategySignals",
                columns: new[] { "Asset", "HourUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategySignals_StrategyName_HourUtc",
                table: "StrategySignals",
                columns: new[] { "StrategyName", "HourUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacktestMetrics");

            migrationBuilder.DropTable(
                name: "BacktestRuns");

            migrationBuilder.DropTable(
                name: "BacktestSteps");

            migrationBuilder.DropTable(
                name: "BacktestTrades");

            migrationBuilder.DropTable(
                name: "DecisionLogs");

            migrationBuilder.DropTable(
                name: "HourlyCandles");

            migrationBuilder.DropTable(
                name: "HourlyFeatures");

            migrationBuilder.DropTable(
                name: "MarketSnapshots");

            migrationBuilder.DropTable(
                name: "PerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "RegimeStates");

            migrationBuilder.DropTable(
                name: "StrategySignals");

            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
