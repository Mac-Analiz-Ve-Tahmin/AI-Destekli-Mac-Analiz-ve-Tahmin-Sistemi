using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchAnalysisSystem.DataAccess.Migrations
{
    public partial class AddUniqueCacheKeyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CacheKey",
                table: "MatchPredictions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_CacheKey",
                table: "MatchPredictions",
                column: "CacheKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchPredictions_CacheKey",
                table: "MatchPredictions");

            migrationBuilder.AlterColumn<string>(
                name: "CacheKey",
                table: "MatchPredictions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
