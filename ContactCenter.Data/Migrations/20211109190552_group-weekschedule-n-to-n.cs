using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class groupweekschedulenton : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeekSchedules_GroupId",
                table: "WeekSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_WeekSchedules_GroupId",
                table: "WeekSchedules",
                column: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeekSchedules_GroupId",
                table: "WeekSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_WeekSchedules_GroupId",
                table: "WeekSchedules",
                column: "GroupId",
                unique: true);
        }
    }
}
