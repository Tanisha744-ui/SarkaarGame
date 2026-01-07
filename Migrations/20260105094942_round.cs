using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarkaar_Apis.Migrations
{
    /// <inheritdoc />
    public partial class round : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "ImposterRoundDecisions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Round",
                table: "ImposterRoundDecisions");
        }
    }
}
