using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchAnalysisSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudPredictionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CacheKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwayTeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedHomeGoals = table.Column<double>(type: "float", nullable: false),
                    ExpectedAwayGoals = table.Column<double>(type: "float", nullable: false),
                    MostLikelyScore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeWinProbability = table.Column<int>(type: "int", nullable: false),
                    DrawProbability = table.Column<int>(type: "int", nullable: false),
                    AwayWinProbability = table.Column<int>(type: "int", nullable: false),
                    Over25Probability = table.Column<int>(type: "int", nullable: false),
                    Under25Probability = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPredictions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchPredictions");
        }
    }
}
