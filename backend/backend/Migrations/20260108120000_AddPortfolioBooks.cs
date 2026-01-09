using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Book",
                table: "Trades",
                type: "TEXT",
                nullable: false,
                defaultValue: "AGENT");

            migrationBuilder.AddColumn<string>(
                name: "Book",
                table: "Portfolios",
                type: "TEXT",
                nullable: false,
                defaultValue: "AGENT");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_Book",
                table: "Portfolios",
                column: "Book",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Portfolios_Book",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Book",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Book",
                table: "Portfolios");
        }
    }
}
