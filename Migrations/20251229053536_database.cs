using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarkaar_Apis.Migrations
{
    /// <inheritdoc />
    public partial class database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImposterGames",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommonWord = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImposterWord = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImposterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsStarted = table.Column<bool>(type: "bit", nullable: false),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    CurrentClueTurnIndex = table.Column<int>(type: "int", nullable: false),
                    CurrentVoteTurnIndex = table.Column<int>(type: "int", nullable: false),
                    CluePhaseComplete = table.Column<bool>(type: "bit", nullable: false),
                    VotePhaseComplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImposterGames", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "ImposterPlayers",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsImposter = table.Column<bool>(type: "bit", nullable: false),
                    Clue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImposterPlayers", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_ImposterPlayers_ImposterGames_GameId",
                        column: x => x.GameId,
                        principalTable: "ImposterGames",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImposterPlayers_GameId",
                table: "ImposterPlayers",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImposterPlayers");

            migrationBuilder.DropTable(
                name: "ImposterGames");
        }
    }
}
