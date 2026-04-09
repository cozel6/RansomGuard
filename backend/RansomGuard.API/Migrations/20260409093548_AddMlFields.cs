using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RansomGuard.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMlFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MlConfidence",
                table: "AnalysisResults",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MlModelVersion",
                table: "AnalysisResults",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MlConfidence",
                table: "AnalysisResults");

            migrationBuilder.DropColumn(
                name: "MlModelVersion",
                table: "AnalysisResults");
        }
    }
}
