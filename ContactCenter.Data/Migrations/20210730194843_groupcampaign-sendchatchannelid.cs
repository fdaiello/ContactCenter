using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class groupcampaignsendchatchannelid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendMsgChatChannelId",
                table: "GroupCampaigns",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_SendMsgChatChannelId",
                table: "GroupCampaigns",
                column: "SendMsgChatChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupCampaigns_ChatChannels_SendMsgChatChannelId",
                table: "GroupCampaigns",
                column: "SendMsgChatChannelId",
                principalTable: "ChatChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupCampaigns_ChatChannels_SendMsgChatChannelId",
                table: "GroupCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_GroupCampaigns_SendMsgChatChannelId",
                table: "GroupCampaigns");

            migrationBuilder.DropColumn(
                name: "SendMsgChatChannelId",
                table: "GroupCampaigns");
        }
    }
}
