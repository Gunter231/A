using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCabinetEducationProgram.Migrations
{
    public partial class AddFileColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "file_path",
                schema: "personal_cabinet",
                table: "educational_program_elements",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                schema: "personal_cabinet",
                table: "educational_program_elements",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_path",
                schema: "personal_cabinet",
                table: "educational_program_elements");

            migrationBuilder.DropColumn(
                name: "file_name",
                schema: "personal_cabinet",
                table: "educational_program_elements");
        }
    }
}
