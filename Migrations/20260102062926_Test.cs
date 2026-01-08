using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarkaar_Apis.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK if exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ImposterVotes_ImposterGames_GameId'
                )
                ALTER TABLE [ImposterVotes] DROP CONSTRAINT [FK_ImposterVotes_ImposterGames_GameId];
            ");

            // Drop FK for ImposterGameId if exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ImposterVotes_ImposterGames_ImposterGameId'
                )
                ALTER TABLE [ImposterVotes] DROP CONSTRAINT [FK_ImposterVotes_ImposterGames_ImposterGameId];
            ");

            // Drop index for GameId if exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes WHERE name = 'IX_ImposterVotes_GameId'
                )
                DROP INDEX [IX_ImposterVotes_GameId] ON [ImposterVotes];
            ");

            // Drop index for ImposterGameId if exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes WHERE name = 'IX_ImposterVotes_ImposterGameId'
                )
                DROP INDEX [IX_ImposterVotes_ImposterGameId] ON [ImposterVotes];
            ");

            // Drop column if exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns WHERE Name = N'ImposterGameId' AND Object_ID = Object_ID(N'ImposterVotes')
                )
                ALTER TABLE [ImposterVotes] DROP COLUMN [ImposterGameId];
            ");

            // Now add index and FK for GameId
            migrationBuilder.CreateIndex(
                name: "IX_ImposterVotes_GameId",
                table: "ImposterVotes",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImposterVotes_ImposterGames_GameId",
                table: "ImposterVotes",
                column: "GameId",
                principalTable: "ImposterGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImposterVotes_ImposterGames_GameId",
                table: "ImposterVotes");

            migrationBuilder.DropIndex(
                name: "IX_ImposterVotes_GameId",
                table: "ImposterVotes");

            migrationBuilder.AddColumn<Guid>(
                name: "ImposterGameId",
                table: "ImposterVotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImposterVotes_ImposterGameId",
                table: "ImposterVotes",
                column: "ImposterGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImposterVotes_ImposterGames_ImposterGameId",
                table: "ImposterVotes",
                column: "ImposterGameId",
                principalTable: "ImposterGames",
                principalColumn: "Id");
        }
    }
}
