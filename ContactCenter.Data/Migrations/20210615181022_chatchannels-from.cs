using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class chatchannelsfrom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "ChatChannels");

            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "ChatChannels",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "From",
                table: "ChatChannels");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "ChatChannels",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
