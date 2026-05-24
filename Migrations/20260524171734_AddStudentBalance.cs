using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchool.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Teachers");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Students",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "Teachers",
                type: "text",
                nullable: true);
        }
    }
}
