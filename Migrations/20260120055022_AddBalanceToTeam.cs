using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarkaar_Apis.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Teams",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Teams");
        }
    }
}
