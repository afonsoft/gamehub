using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class Poki29 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AverageLatencyMs",
                table: "gh_MatchStates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "gh_MatchStates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRanked",
                table: "gh_MatchStates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RankedSeasonId",
                table: "gh_MatchStates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "gh_MatchStates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gh_MatchHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WinnerUserId = table.Column<long>(type: "bigint", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResultsJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_MatchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_MultiplayerSecurityEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ConnectionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_MultiplayerSecurityEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_PlayerRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_PlayerRatings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_RankedQueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Region = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RatingSnapshot = table.Column<int>(type: "integer", nullable: false),
                    EnqueuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_RankedQueueEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_RankedSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_RankedSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_ReplayMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    MatchHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_ReplayMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_ReplayMetadata_gh_MatchHistories_MatchHistoryId",
                        column: x => x.MatchHistoryId,
                        principalTable: "gh_MatchHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchHistories_GameId_EndedAt",
                table: "gh_MatchHistories",
                columns: new[] { "GameId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchHistories_MatchId",
                table: "gh_MatchHistories",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_MultiplayerSecurityEvents_GameId_OccurredAt",
                table: "gh_MultiplayerSecurityEvents",
                columns: new[] { "GameId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_MultiplayerSecurityEvents_MatchId_EventType",
                table: "gh_MultiplayerSecurityEvents",
                columns: new[] { "MatchId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerRatings_GameId_SeasonId_Mode_UserId",
                table: "gh_PlayerRatings",
                columns: new[] { "GameId", "SeasonId", "Mode", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_RankedQueueEntries_GameId_SeasonId_Mode_Region_Status_En~",
                table: "gh_RankedQueueEntries",
                columns: new[] { "GameId", "SeasonId", "Mode", "Region", "Status", "EnqueuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_RankedQueueEntries_UserId_Status",
                table: "gh_RankedQueueEntries",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_RankedSeasons_GameId_Mode_IsActive",
                table: "gh_RankedSeasons",
                columns: new[] { "GameId", "Mode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_ReplayMetadata_ExpiresAt",
                table: "gh_ReplayMetadata",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_gh_ReplayMetadata_MatchHistoryId",
                table: "gh_ReplayMetadata",
                column: "MatchHistoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_MultiplayerSecurityEvents");

            migrationBuilder.DropTable(
                name: "gh_PlayerRatings");

            migrationBuilder.DropTable(
                name: "gh_RankedQueueEntries");

            migrationBuilder.DropTable(
                name: "gh_RankedSeasons");

            migrationBuilder.DropTable(
                name: "gh_ReplayMetadata");

            migrationBuilder.DropTable(
                name: "gh_MatchHistories");

            migrationBuilder.DropColumn(
                name: "AverageLatencyMs",
                table: "gh_MatchStates");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "gh_MatchStates");

            migrationBuilder.DropColumn(
                name: "IsRanked",
                table: "gh_MatchStates");

            migrationBuilder.DropColumn(
                name: "RankedSeasonId",
                table: "gh_MatchStates");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "gh_MatchStates");
        }
    }
}
