using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoAgent.Api.Migrations;

[DbContext(typeof(CryptoAgent.Api.Data.CryptoAgentDbContext))]
[Migration("20260204120000_AddExogenousThemeScoring")]
public partial class AddExogenousThemeScoring : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "GatingReasonJson",
            table: "DecisionInputsExogenous",
            type: "TEXT",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AddColumn<string>(
            name: "MarketAlignmentJson",
            table: "DecisionInputsExogenous",
            type: "TEXT",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AddColumn<decimal>(
            name: "PositionSizeModifier",
            table: "DecisionInputsExogenous",
            type: "TEXT",
            nullable: false,
            defaultValue: 1m);

        migrationBuilder.AddColumn<string>(
            name: "ThemeConflictJson",
            table: "DecisionInputsExogenous",
            type: "TEXT",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AddColumn<string>(
            name: "ThemeDirectionJson",
            table: "DecisionInputsExogenous",
            type: "TEXT",
            nullable: false,
            defaultValue: "{}");

        migrationBuilder.AddColumn<string>(
            name: "ThemeStrengthJson",
            table: "DecisionInputsExogenous",
            type: "TEXT",
            nullable: false,
            defaultValue: "{}");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "GatingReasonJson",
            table: "DecisionInputsExogenous");

        migrationBuilder.DropColumn(
            name: "MarketAlignmentJson",
            table: "DecisionInputsExogenous");

        migrationBuilder.DropColumn(
            name: "PositionSizeModifier",
            table: "DecisionInputsExogenous");

        migrationBuilder.DropColumn(
            name: "ThemeConflictJson",
            table: "DecisionInputsExogenous");

        migrationBuilder.DropColumn(
            name: "ThemeDirectionJson",
            table: "DecisionInputsExogenous");

        migrationBuilder.DropColumn(
            name: "ThemeStrengthJson",
            table: "DecisionInputsExogenous");
    }
}
