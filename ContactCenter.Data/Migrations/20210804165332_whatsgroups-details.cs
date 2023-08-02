using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class whatsgroupsdetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Created",
                table: "WhatsGroups",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ImageSet",
                table: "WhatsGroups",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "WhatsGroups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Obs",
                table: "WhatsGroups",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "WhatsGroups");

            migrationBuilder.DropColumn(
                name: "ImageSet",
                table: "WhatsGroups");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "WhatsGroups");

            migrationBuilder.DropColumn(
                name: "Obs",
                table: "WhatsGroups");
        }
    }
}
