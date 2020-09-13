using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pulse.Backend.Migrations
{
    public partial class Schotten2Log : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Schotten2Games",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.CreateTable(
                name: "Schotten2Logs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GameId = table.Column<int>(nullable: false),
                    Player = table.Column<string>(maxLength: 256, nullable: true),
                    Action = table.Column<string>(maxLength: 256, nullable: true),
                    HandIndex = table.Column<int>(nullable: true),
                    SectionIndex = table.Column<int>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schotten2Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schotten2Logs_Schotten2Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Schotten2Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schotten2Logs_GameId",
                table: "Schotten2Logs",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schotten2Logs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Schotten2Games",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
