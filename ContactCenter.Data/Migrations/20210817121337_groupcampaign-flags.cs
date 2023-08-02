using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class groupcampaignflags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreateErrorCount",
                table: "WhatsGroups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SetImageErrorCount",
                table: "WhatsGroups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ChangeAdmins",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ChangeDescription",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ChangeImage",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ChangePermissions",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Updating",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateErrorCount",
                table: "WhatsGroups");

            migrationBuilder.DropColumn(
                name: "SetImageErrorCount",
                table: "WhatsGroups");

            migrationBuilder.DropColumn(
                name: "ChangeAdmins",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "ChangeDescription",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "ChangeImage",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "ChangePermissions",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "Updating",
                table: "GroupCampaigns");
        }
    }
}
