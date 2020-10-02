using Microsoft.EntityFrameworkCore.Migrations;

namespace Pulse.Backend.Migrations
{
    public partial class Schotten2_Resigned_And_Expired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchName",
                table: "Schotten2Games");

            migrationBuilder.AddColumn<string>(
                name: "ExpiredId",
                table: "Schotten2Games",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResignedId",
                table: "Schotten2Games",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredId",
                table: "Schotten2Games");

            migrationBuilder.DropColumn(
                name: "ResignedId",
                table: "Schotten2Games");

            migrationBuilder.AddColumn<string>(
                name: "MatchName",
                table: "Schotten2Games",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
