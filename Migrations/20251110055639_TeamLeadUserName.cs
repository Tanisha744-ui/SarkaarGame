using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarkaar_Apis.Migrations
{
    /// <inheritdoc />
    public partial class TeamLeadUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamLeadUsername",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamLeadUsername",
                table: "Teams");
        }
    }
}
