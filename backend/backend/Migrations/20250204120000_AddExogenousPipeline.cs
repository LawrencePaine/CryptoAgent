using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddExogenousPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExogenousSummary",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExogenousTraceJson",
                table: "DecisionLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.CreateTable(
                name: "DecisionInputsExogenous",
                columns: table => new
                {
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ThemeScoresJson = table.Column<string>(type: "TEXT", nullable: false),
                    AlignmentFlagsJson = table.Column<string>(type: "TEXT", nullable: false),
                    AbstainModifier = table.Column<decimal>(type: "TEXT", nullable: false),
                    ConfidenceThresholdModifier = table.Column<decimal>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    TraceIdsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionInputsExogenous", x => x.TimestampUtc);
                });

            migrationBuilder.CreateTable(
                name: "ExogenousClassifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ThemeRelevance = table.Column<string>(type: "TEXT", nullable: false),
                    ImpactHorizon = table.Column<string>(type: "TEXT", nullable: false),
                    DirectionalBias = table.Column<string>(type: "TEXT", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    NoveltyScore = table.Column<decimal>(type: "TEXT", nullable: true),
                    SummaryBulletsJson = table.Column<string>(type: "TEXT", nullable: false),
                    KeyEntitiesJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExogenousClassifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExogenousItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceCredibilityWeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContentHash = table.Column<string>(type: "TEXT", nullable: false),
                    RawExcerpt = table.Column<string>(type: "TEXT", nullable: false),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExogenousItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Narratives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    SeedText = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StateScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    DirectionalBias = table.Column<string>(type: "TEXT", nullable: false),
                    Horizon = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Narratives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NarrativeItems",
                columns: table => new
                {
                    NarrativeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContributionWeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NarrativeItems", x => new { x.NarrativeId, x.ItemId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionInputsExogenous_TimestampUtc",
                table: "DecisionInputsExogenous",
                column: "TimestampUtc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExogenousClassifications_CreatedAt",
                table: "ExogenousClassifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExogenousClassifications_ItemId",
                table: "ExogenousClassifications",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExogenousClassifications_ThemeRelevance",
                table: "ExogenousClassifications",
                column: "ThemeRelevance");

            migrationBuilder.CreateIndex(
                name: "IX_ExogenousItems_PublishedAt",
                table: "ExogenousItems",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExogenousItems_Status",
                table: "ExogenousItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExogenousItems_Url",
                table: "ExogenousItems",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeItems_ItemId",
                table: "NarrativeItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeItems_NarrativeId",
                table: "NarrativeItems",
                column: "NarrativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Narratives_LastUpdatedAt",
                table: "Narratives",
                column: "LastUpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Narratives_Theme",
                table: "Narratives",
                column: "Theme");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionInputsExogenous");

            migrationBuilder.DropTable(
                name: "ExogenousClassifications");

            migrationBuilder.DropTable(
                name: "ExogenousItems");

            migrationBuilder.DropTable(
                name: "NarrativeItems");

            migrationBuilder.DropTable(
                name: "Narratives");

            migrationBuilder.DropColumn(
                name: "ExogenousSummary",
                table: "DecisionLogs");

            migrationBuilder.DropColumn(
                name: "ExogenousTraceJson",
                table: "DecisionLogs");
        }
    }
}
