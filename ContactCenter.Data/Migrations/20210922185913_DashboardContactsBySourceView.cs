using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class DashboardContactsBySourceView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardContactsBySourceView",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 256, nullable: false),
                    ChannelName = table.Column<string>(maxLength: 256, nullable: true),
                    ChannelNumber = table.Column<string>(maxLength: 256, nullable: true),
                    SourceDescription = table.Column<string>(maxLength: 256, nullable: true),
                    Qtd = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardContactsBySourceView", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardContactsBySourceView");
        }
    }
}
