using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class GroupCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    ImageFileName = table.Column<string>(maxLength: 256, nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    FacePixelCode = table.Column<string>(maxLength: 128, nullable: true),
                    GoogleAdsCode = table.Column<string>(maxLength: 256, nullable: true),
                    ClosedUrl = table.Column<string>(maxLength: 256, nullable: true),
                    Clicks = table.Column<int>(nullable: false),
                    Leads = table.Column<int>(nullable: false),
                    Members = table.Column<int>(nullable: false),
                    EnterUrlSufix = table.Column<string>(maxLength: 64, nullable: true),
                    MessageId = table.Column<int>(nullable: false),
                    ChatChannelId = table.Column<string>(maxLength: 32, nullable: true),
                    GroupBoardId = table.Column<int>(nullable: true),
                    LeadsListId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupCampaigns_ChatChannels_ChatChannelId",
                        column: x => x.ChatChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupCampaigns_Boards_GroupBoardId",
                        column: x => x.GroupBoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupCampaigns_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupCampaigns_Boards_LeadsListId",
                        column: x => x.LeadsListId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupCampaigns_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_ChatChannelId",
                table: "GroupCampaigns",
                column: "ChatChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_GroupBoardId",
                table: "GroupCampaigns",
                column: "GroupBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_GroupId",
                table: "GroupCampaigns",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_LeadsListId",
                table: "GroupCampaigns",
                column: "LeadsListId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCampaigns_MessageId",
                table: "GroupCampaigns",
                column: "MessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupCampaigns");
        }
    }
}
