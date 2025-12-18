using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionValuationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BtcCostBasisGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BtcUnrealisedPnlGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BtcValueGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EthCostBasisGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EthUnrealisedPnlGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EthValueGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalValueGbp",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BtcCostBasisGbp",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "BtcUnrealisedPnlGbp",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "BtcValueGbp",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "EthCostBasisGbp",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "EthUnrealisedPnlGbp",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "EthValueGbp",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "TotalValueGbp",
                table: "DecisionLogs");
        }
    }
}
