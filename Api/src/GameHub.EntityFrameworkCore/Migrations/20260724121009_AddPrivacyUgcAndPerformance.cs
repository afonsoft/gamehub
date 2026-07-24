using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyUgcAndPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameId1",
                table: "gh_RevenueContracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CommercialBreakCount",
                table: "gh_PlaySessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "FpsAverage",
                table: "gh_PlaySessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FpsMin",
                table: "gh_PlaySessions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RewardedBreakCount",
                table: "gh_PlaySessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyUrl",
                table: "gh_Games",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AvgFps",
                table: "gh_GameMetricSnapshots",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinFps",
                table: "gh_GameMetricSnapshots",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "gh_Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "gh_Categories",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasExternalRequests",
                table: "gh_BuildValidationReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "gh_InspectorSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameBuildId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DevicePreset = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Resolution = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
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
                    table.PrimaryKey("PK_gh_InspectorSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_InspectorSessions_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_PlayerFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_gh_PlayerFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_PlayerFavorites_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gh_PlayerFavorites_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_PlayerRecentGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    LastPlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
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
                    table.PrimaryKey("PK_gh_PlayerRecentGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_PlayerRecentGames_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gh_PlayerRecentGames_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_UserContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresModeration = table.Column<bool>(type: "boolean", nullable: false),
                    ModerationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_gh_UserContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_UserContents_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_InspectorSdkEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_InspectorSdkEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_InspectorSdkEvents_gh_InspectorSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "gh_InspectorSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_InspectorWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_InspectorWarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_InspectorWarnings_gh_InspectorSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "gh_InspectorSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_RevenueContracts_GameId1",
                table: "gh_RevenueContracts",
                column: "GameId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_InspectorSdkEvents_SessionId_SequenceNumber",
                table: "gh_InspectorSdkEvents",
                columns: new[] { "SessionId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_InspectorSessions_GameId_StartedAt",
                table: "gh_InspectorSessions",
                columns: new[] { "GameId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_InspectorWarnings_SessionId",
                table: "gh_InspectorWarnings",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerFavorites_GameId_UserId",
                table: "gh_PlayerFavorites",
                columns: new[] { "GameId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerFavorites_UserId_CreationTime",
                table: "gh_PlayerFavorites",
                columns: new[] { "UserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerRecentGames_GameId_UserId",
                table: "gh_PlayerRecentGames",
                columns: new[] { "GameId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerRecentGames_UserId_LastPlayedAt",
                table: "gh_PlayerRecentGames",
                columns: new[] { "UserId", "LastPlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_UserContents_GameId_IsApproved_RequiresModeration",
                table: "gh_UserContents",
                columns: new[] { "GameId", "IsApproved", "RequiresModeration" });

            migrationBuilder.AddForeignKey(
                name: "FK_gh_RevenueContracts_gh_Games_GameId1",
                table: "gh_RevenueContracts",
                column: "GameId1",
                principalTable: "gh_Games",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gh_RevenueContracts_gh_Games_GameId1",
                table: "gh_RevenueContracts");

            migrationBuilder.DropTable(
                name: "gh_InspectorSdkEvents");

            migrationBuilder.DropTable(
                name: "gh_InspectorWarnings");

            migrationBuilder.DropTable(
                name: "gh_PlayerFavorites");

            migrationBuilder.DropTable(
                name: "gh_PlayerRecentGames");

            migrationBuilder.DropTable(
                name: "gh_UserContents");

            migrationBuilder.DropTable(
                name: "gh_InspectorSessions");

            migrationBuilder.DropIndex(
                name: "IX_gh_RevenueContracts_GameId1",
                table: "gh_RevenueContracts");

            migrationBuilder.DropColumn(
                name: "GameId1",
                table: "gh_RevenueContracts");

            migrationBuilder.DropColumn(
                name: "CommercialBreakCount",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "FpsAverage",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "FpsMin",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "RewardedBreakCount",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyUrl",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "AvgFps",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "MinFps",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "gh_Categories");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "gh_Categories");

            migrationBuilder.DropColumn(
                name: "HasExternalRequests",
                table: "gh_BuildValidationReports");
        }
    }
}
