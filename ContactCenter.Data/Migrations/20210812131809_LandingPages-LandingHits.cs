using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class LandingPagesLandingHits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Landings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(maxLength: 256, nullable: true),
                    Html = table.Column<string>(nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    Code = table.Column<string>(maxLength: 64, nullable: true),
                    Index = table.Column<int>(nullable: true),
                    PageViews = table.Column<int>(nullable: false),
                    Leads = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Landings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandingHits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LandingId = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    HitType = table.Column<int>(nullable: false),
                    ContactId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingHits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingHits_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LandingHits_Landings_LandingId",
                        column: x => x.LandingId,
                        principalTable: "Landings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LandingHits_ContactId",
                table: "LandingHits",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_LandingHits_LandingId",
                table: "LandingHits",
                column: "LandingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingHits");

            migrationBuilder.DropTable(
                name: "Landings");
        }
    }
}
