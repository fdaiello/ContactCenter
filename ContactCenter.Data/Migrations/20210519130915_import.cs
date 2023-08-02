using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class import : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Import",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(nullable: false),
                    BoardId = table.Column<int>(nullable: false),
                    FileName = table.Column<string>(maxLength: 255, nullable: false),
                    Status = table.Column<string>(maxLength: 1, nullable: false),
                    ProgressPercent = table.Column<int>(nullable: false),
                    MsgErro = table.Column<string>(maxLength: 1024, nullable: false),
                    countImported = table.Column<int>(nullable: false),
                    countErrors = table.Column<int>(nullable: false),
                    countTotal = table.Column<int>(nullable: false),
                    DataImport = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Import", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Import_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Import_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Import_BoardId",
                table: "Import",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Import_GroupId",
                table: "Import",
                column: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Import");
        }
    }
}
