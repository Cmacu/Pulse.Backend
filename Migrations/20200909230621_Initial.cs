using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pulse.Backend.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppError",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(maxLength: 256, nullable: true),
                    Details = table.Column<string>(maxLength: 10000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppError", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    To = table.Column<string>(maxLength: 256, nullable: true),
                    Subject = table.Column<string>(maxLength: 256, nullable: true),
                    Body = table.Column<string>(maxLength: 10000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Status = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakerLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    AddedAt = table.Column<DateTime>(nullable: false),
                    Rating = table.Column<double>(nullable: false),
                    RecentOpponents = table.Column<string>(maxLength: 256, nullable: true),
                    ExpiredAt = table.Column<DateTime>(nullable: true),
                    MatchId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchmakerLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakerLogAggregate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    From = table.Column<DateTime>(nullable: false),
                    To = table.Column<DateTime>(nullable: false),
                    PlayerCount = table.Column<int>(nullable: false),
                    MatchCount = table.Column<int>(nullable: false),
                    LogCount = table.Column<int>(nullable: false),
                    WaitSeconds = table.Column<double>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchmakerLogAggregate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakerLogCounter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    From = table.Column<DateTime>(nullable: false),
                    To = table.Column<DateTime>(nullable: false),
                    PlayerCount = table.Column<int>(nullable: false),
                    MatchCount = table.Column<int>(nullable: false),
                    LogCount = table.Column<int>(nullable: false),
                    WaitSeconds = table.Column<double>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchmakerLogCounter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    AccessCode = table.Column<string>(maxLength: 256, nullable: true),
                    RequestCount = table.Column<int>(nullable: false),
                    Avatar = table.Column<string>(maxLength: 256, nullable: true),
                    Country = table.Column<string>(maxLength: 256, nullable: true),
                    Division = table.Column<int>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    RatingMean = table.Column<double>(nullable: false),
                    RatingDeviation = table.Column<double>(nullable: false),
                    IsBlockedUntil = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    ConservativeRating = table.Column<double>(nullable: false),
                    TotalDecay = table.Column<int>(nullable: false),
                    Rank = table.Column<int>(nullable: false),
                    PreviousRank = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardLog_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchPlayer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MatchId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    Position = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false),
                    IsWin = table.Column<bool>(nullable: false),
                    OldRatingMean = table.Column<double>(nullable: false),
                    OldRatingDeviation = table.Column<double>(nullable: false),
                    NewRatingMean = table.Column<double>(nullable: false),
                    NewRatingDeviation = table.Column<double>(nullable: false),
                    RatingDelta = table.Column<double>(nullable: false),
                    DecayDays = table.Column<int>(nullable: false),
                    DecayValue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPlayer_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchPlayer_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBadge",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBadge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerBadge_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSession",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    RefreshToken = table.Column<string>(maxLength: 256, nullable: true),
                    IpAddress = table.Column<string>(maxLength: 256, nullable: true),
                    Browser = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSession_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSetting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Value = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSetting_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardLog_CreatedAt",
                table: "LeaderboardLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardLog_PlayerId",
                table: "LeaderboardLog",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardLog_CreatedAt_PlayerId",
                table: "LeaderboardLog",
                columns: new[] { "CreatedAt", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakerLogAggregate_From",
                table: "MatchmakerLogAggregate",
                column: "From",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakerLogCounter_From",
                table: "MatchmakerLogCounter",
                column: "From",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayer_MatchId",
                table: "MatchPlayer",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayer_PlayerId",
                table: "MatchPlayer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_Email",
                table: "Player",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBadge_PlayerId",
                table: "PlayerBadge",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSession_PlayerId",
                table: "PlayerSession",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSetting_PlayerId",
                table: "PlayerSetting",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppError");

            migrationBuilder.DropTable(
                name: "EmailLog");

            migrationBuilder.DropTable(
                name: "LeaderboardLog");

            migrationBuilder.DropTable(
                name: "MatchmakerLog");

            migrationBuilder.DropTable(
                name: "MatchmakerLogAggregate");

            migrationBuilder.DropTable(
                name: "MatchmakerLogCounter");

            migrationBuilder.DropTable(
                name: "MatchPlayer");

            migrationBuilder.DropTable(
                name: "PlayerBadge");

            migrationBuilder.DropTable(
                name: "PlayerSession");

            migrationBuilder.DropTable(
                name: "PlayerSetting");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "Player");
        }
    }
}
