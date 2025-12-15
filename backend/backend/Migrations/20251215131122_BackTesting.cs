using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class BackTesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacktestMetrics");

            migrationBuilder.DropTable(
                name: "BacktestSteps");

            migrationBuilder.DropTable(
                name: "BacktestTrades");

            migrationBuilder.DropTable(
                name: "BacktestRuns");
        }
    }
}
