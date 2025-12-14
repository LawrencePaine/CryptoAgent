using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyIntelligenceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "HourlyCandles");

            migrationBuilder.DropTable(
                name: "HourlyFeatures");

            migrationBuilder.DropTable(
                name: "RegimeStates");

            migrationBuilder.DropTable(
                name: "StrategySignals");
        }
    }
}
