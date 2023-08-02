using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactCenter.Data.Migrations
{
    public partial class ImportDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Import_Boards_BoardId",
                table: "Import");

            migrationBuilder.DropForeignKey(
                name: "FK_Import_Groups_GroupId",
                table: "Import");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Import",
                table: "Import");

            migrationBuilder.DropColumn(
                name: "DataImport",
                table: "Import");

            migrationBuilder.RenameTable(
                name: "Import",
                newName: "Imports");

            migrationBuilder.RenameIndex(
                name: "IX_Import_GroupId",
                table: "Imports",
                newName: "IX_Imports_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Import_BoardId",
                table: "Imports",
                newName: "IX_Imports_BoardId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Imports",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportDate",
                table: "Imports",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Imports",
                table: "Imports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Imports_Boards_BoardId",
                table: "Imports",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Imports_Groups_GroupId",
                table: "Imports",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imports_Boards_BoardId",
                table: "Imports");

            migrationBuilder.DropForeignKey(
                name: "FK_Imports_Groups_GroupId",
                table: "Imports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Imports",
                table: "Imports");

            migrationBuilder.DropColumn(
                name: "ImportDate",
                table: "Imports");

            migrationBuilder.RenameTable(
                name: "Imports",
                newName: "Import");

            migrationBuilder.RenameIndex(
                name: "IX_Imports_GroupId",
                table: "Import",
                newName: "IX_Import_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Imports_BoardId",
                table: "Import",
                newName: "IX_Import_BoardId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Import",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataImport",
                table: "Import",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Import",
                table: "Import",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Import_Boards_BoardId",
                table: "Import",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Import_Groups_GroupId",
                table: "Import",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
