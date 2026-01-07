using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ExogeneousScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ThemeStrengthJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "ThemeDirectionJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "ThemeConflictJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<decimal>(
                name: "PositionSizeModifier",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldDefaultValue: 1m);

            migrationBuilder.AlterColumn<string>(
                name: "MarketAlignmentJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "GatingReasonJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ThemeStrengthJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ThemeDirectionJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ThemeConflictJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "PositionSizeModifier",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                defaultValue: 1m,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "MarketAlignmentJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "GatingReasonJson",
                table: "DecisionInputsExogenous",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
