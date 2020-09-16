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
                name: "AppErrors",
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
                    table.PrimaryKey("PK_AppErrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLogs",
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
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
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
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakerLogAggregates",
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
                    table.PrimaryKey("PK_MatchmakerLogAggregates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakerLogCounters",
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
                    table.PrimaryKey("PK_MatchmakerLogCounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakerLogs",
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
                    table.PrimaryKey("PK_MatchmakerLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
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
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schotten2Games",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MatchId = table.Column<string>(maxLength: 256, nullable: true),
                    AttackerId = table.Column<string>(maxLength: 256, nullable: true),
                    DefenderId = table.Column<string>(maxLength: 256, nullable: true),
                    WinnerId = table.Column<string>(maxLength: 256, nullable: true),
                    State = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schotten2Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schotten2Logs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MatchId = table.Column<string>(maxLength: 256, nullable: true),
                    PlayerId = table.Column<string>(maxLength: 256, nullable: true),
                    Action = table.Column<string>(maxLength: 256, nullable: true),
                    HandIndex = table.Column<int>(nullable: true),
                    SectionIndex = table.Column<int>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schotten2Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardLogs",
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
                    table.PrimaryKey("PK_LeaderboardLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardLogs_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchPlayers",
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
                    table.PrimaryKey("PK_MatchPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPlayers_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBadges",
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
                    table.PrimaryKey("PK_PlayerBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerBadges_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSettings",
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
                    table.PrimaryKey("PK_PlayerSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSettings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
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
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardLogs_CreatedAt",
                table: "LeaderboardLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardLogs_PlayerId",
                table: "LeaderboardLogs",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardLogs_CreatedAt_PlayerId",
                table: "LeaderboardLogs",
                columns: new[] { "CreatedAt", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakerLogAggregates_From",
                table: "MatchmakerLogAggregates",
                column: "From",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakerLogCounters_From",
                table: "MatchmakerLogCounters",
                column: "From",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_MatchId",
                table: "MatchPlayers",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_PlayerId",
                table: "MatchPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBadges_PlayerId",
                table: "PlayerBadges",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSettings_PlayerId",
                table: "PlayerSettings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Schotten2Games_MatchId",
                table: "Schotten2Games",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schotten2Logs_MatchId",
                table: "Schotten2Logs",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlayerId",
                table: "Sessions",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppErrors");

            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "LeaderboardLogs");

            migrationBuilder.DropTable(
                name: "MatchmakerLogAggregates");

            migrationBuilder.DropTable(
                name: "MatchmakerLogCounters");

            migrationBuilder.DropTable(
                name: "MatchmakerLogs");

            migrationBuilder.DropTable(
                name: "MatchPlayers");

            migrationBuilder.DropTable(
                name: "PlayerBadges");

            migrationBuilder.DropTable(
                name: "PlayerSettings");

            migrationBuilder.DropTable(
                name: "Schotten2Games");

            migrationBuilder.DropTable(
                name: "Schotten2Logs");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
