using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class BotSettingsMainDialogTranferModeApplicationUserLastCAllTransfered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BotCallAgentsMode",
                table: "BotSettings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BotMainDialog",
                table: "BotSettings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastContactTransfered",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotCallAgentsMode",
                table: "BotSettings");

            migrationBuilder.DropColumn(
                name: "BotMainDialog",
                table: "BotSettings");

            migrationBuilder.DropColumn(
                name: "LastContactTransfered",
                table: "AspNetUsers");
        }
    }
}
