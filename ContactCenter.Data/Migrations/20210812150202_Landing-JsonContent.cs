using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class LandingJsonContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonContent",
                table: "Landings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonContent",
                table: "Landings");
        }
    }
}
