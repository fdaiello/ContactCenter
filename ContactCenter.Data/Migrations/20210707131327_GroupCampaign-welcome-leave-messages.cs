using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class GroupCampaignwelcomeleavemessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeaveMessageId",
                table: "GroupCampaigns",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WelcomeMessageId",
                table: "GroupCampaigns",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_LeaveMessageId",
                table: "GroupCampaigns",
                column: "LeaveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_WelcomeMessageId",
                table: "GroupCampaigns",
                column: "WelcomeMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupCampaigns_Messages_LeaveMessageId",
                table: "GroupCampaigns",
                column: "LeaveMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupCampaigns_Messages_WelcomeMessageId",
                table: "GroupCampaigns",
                column: "WelcomeMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCampaigns_Messages_LeaveMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupCampaigns_Messages_WelcomeMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_GroupCampaigns_LeaveMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_GroupCampaigns_WelcomeMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "LeaveMessageId",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "WelcomeMessageId",
                table: "GroupCampaigns");
        }
    }
}
