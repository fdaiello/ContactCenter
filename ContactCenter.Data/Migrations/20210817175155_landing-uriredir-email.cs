using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class landingurirediremail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAlert",
                table: "Landings",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedirUri",
                table: "Landings",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAlert",
                table: "Landings");

            migrationBuilder.DropColumn(
                name: "RedirUri",
                table: "Landings");
        }
    }
}
