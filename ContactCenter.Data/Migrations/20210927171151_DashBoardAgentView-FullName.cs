using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class DashBoardAgentViewFullName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NickName",
                table: "DashboardAgentViews");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "DashboardAgentViews",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "DashboardAgentViews");

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "DashboardAgentViews",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
