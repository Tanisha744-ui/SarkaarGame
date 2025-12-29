using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarkaar_Apis.Migrations
{
    /// <inheritdoc />
    public partial class imposter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImposterPlayers_ImposterGames_GameId",
                table: "ImposterPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImposterGames",
                table: "ImposterGames");

            migrationBuilder.RenameTable(
                name: "ImposterGames",
                newName: "Games");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Games",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Games",
                table: "Games",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImposterPlayers_Games_GameId",
                table: "ImposterPlayers",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImposterPlayers_Games_GameId",
                table: "ImposterPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Games",
                table: "Games");

            migrationBuilder.RenameTable(
                name: "Games",
                newName: "ImposterGames");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ImposterGames",
                newName: "GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImposterGames",
                table: "ImposterGames",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImposterPlayers_ImposterGames_GameId",
                table: "ImposterPlayers",
                column: "GameId",
                principalTable: "ImposterGames",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
