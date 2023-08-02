using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class MessagesSmartCode64lenghtGroupCampaignStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SmartCode",
                table: "Messages",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Obs",
                table: "GroupCampaigns",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Obs",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GroupCampaigns");

            migrationBuilder.AlterColumn<string>(
                name: "SmartCode",
                table: "Messages",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
