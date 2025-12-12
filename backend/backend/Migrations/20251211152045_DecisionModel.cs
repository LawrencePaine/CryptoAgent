using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class DecisionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    FinalAction = table.Column<string>(type: "TEXT", nullable: false),
                    FinalAsset = table.Column<string>(type: "TEXT", nullable: false),
                    FinalSizeGbp = table.Column<decimal>(type: "TEXT", nullable: false),
                    Executed = table.Column<bool>(type: "INTEGER", nullable: false),
                    RationaleShort = table.Column<string>(type: "TEXT", nullable: false),
                    RiskReason = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionLogs");
        }
    }
}
