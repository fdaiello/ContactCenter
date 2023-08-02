using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class BotSettingsAskNamePhraseEnableCheckIntentTransferAgentPhrase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AskNamePhrase",
                table: "BotSettings",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableCheckIntent",
                table: "BotSettings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransferAgentPhrase",
                table: "BotSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AskNamePhrase",
                table: "BotSettings");

            migrationBuilder.DropColumn(
                name: "EnableCheckIntent",
                table: "BotSettings");

            migrationBuilder.DropColumn(
                name: "TransferAgentPhrase",
                table: "BotSettings");
        }
    }
}
