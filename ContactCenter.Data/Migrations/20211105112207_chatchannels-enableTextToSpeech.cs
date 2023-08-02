using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class chatchannelsenableTextToSpeech : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableTextToSpeech",
                table: "ChatChannels",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableTextToSpeech",
                table: "ChatChannels");
        }
    }
}
