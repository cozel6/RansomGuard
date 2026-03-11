using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RansomGuard.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RiskScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Entropy = table.Column<double>(type: "REAL", nullable: false),
                    SuspiciousAPIs = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Verdict = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SectionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ExportCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_FileHash",
                table: "AnalysisResults",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_Timestamp",
                table: "AnalysisResults",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisResults");
        }
    }
}
