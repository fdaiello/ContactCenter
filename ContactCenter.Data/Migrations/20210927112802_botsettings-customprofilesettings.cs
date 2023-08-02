using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class botsettingscustomprofilesettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profile2Settings",
                table: "BotSettings");

            migrationBuilder.AddColumn<string>(
                name: "CustomProfileSettings",
                table: "BotSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomProfileSettings",
                table: "BotSettings");

            migrationBuilder.AddColumn<string>(
                name: "Profile2Settings",
                table: "BotSettings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
