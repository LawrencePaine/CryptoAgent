using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LlmConfidence",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProviderUsed",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawModelOutput",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LlmConfidence",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "ProviderUsed",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "RawModelOutput",
                table: "DecisionLogs");
        }
    }
}
