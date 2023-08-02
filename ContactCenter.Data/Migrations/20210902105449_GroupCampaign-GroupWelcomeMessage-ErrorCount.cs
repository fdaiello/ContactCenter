using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class GroupCampaignGroupWelcomeMessageErrorCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ErrorsCount",
                table: "GroupCampaigns",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupWelcomeMessageId",
                table: "GroupCampaigns",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_GroupWelcomeMessageId",
                table: "GroupCampaigns",
                column: "GroupWelcomeMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupCampaigns_Messages_GroupWelcomeMessageId",
                table: "GroupCampaigns",
                column: "GroupWelcomeMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCampaigns_Messages_GroupWelcomeMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_GroupCampaigns_GroupWelcomeMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "ErrorsCount",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "GroupWelcomeMessageId",
                table: "GroupCampaigns");
        }
    }
}
