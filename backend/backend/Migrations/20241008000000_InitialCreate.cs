using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoAgent.Api.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BtcPriceGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthPriceGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcChange24hPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    EthChange24hPct = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcChange7dPct = table.Column<decimal>(type: "TEXT", nullable: false),
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
                name: "IX_Portfolios_Id",
                table: "Portfolios",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketSnapshots");

            migrationBuilder.DropTable(
                name: "PerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
