using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class groupcampaignslinksufix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnterUrlSufix",
                table: "GroupCampaigns");

            migrationBuilder.AddColumn<string>(
                name: "LinkSufix",
                table: "GroupCampaigns",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkSufix",
                table: "GroupCampaigns");

            migrationBuilder.AddColumn<string>(
                name: "EnterUrlSufix",
                table: "GroupCampaigns",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
