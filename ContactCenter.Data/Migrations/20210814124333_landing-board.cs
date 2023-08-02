using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class landingboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoardId",
                table: "Landings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Landings_BoardId",
                table: "Landings",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Landings_Boards_BoardId",
                table: "Landings",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Landings_Boards_BoardId",
                table: "Landings");

            migrationBuilder.DropIndex(
                name: "IX_Landings_BoardId",
                table: "Landings");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "Landings");
        }
    }
}
