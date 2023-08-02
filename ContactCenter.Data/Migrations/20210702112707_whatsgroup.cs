using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class whatsgroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCampaignId = table.Column<int>(nullable: false),
                    Wid = table.Column<string>(maxLength: 256, nullable: true),
                    InviteUrl = table.Column<string>(maxLength: 256, nullable: true),
                    Clicks = table.Column<int>(nullable: false),
                    Leads = table.Column<int>(nullable: false),
                    Members = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsGroups_GroupCampaigns_GroupCampaignId",
                        column: x => x.GroupCampaignId,
                        principalTable: "GroupCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsGroups_GroupCampaignId",
                table: "WhatsGroups",
                column: "GroupCampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsGroups");
        }
    }
}
