using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class Poki26 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "gh_UserContents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevelEvents",
                table: "gh_PlaytestRecordings",
                type: "character varying(16000)",
                maxLength: 16000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "gh_GameMetricSnapshots",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DailyPlayingUsers",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "GameplayStartedCount",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LoadingStartedCount",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PageViews",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReviewCount",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "gh_AdImpressions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    DeviceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Cpm = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Earnings = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_gh_AdImpressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_AdImpressions_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_ExternalResourceExemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PrivacyStatementUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModeratorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_gh_ExternalResourceExemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_ExternalResourceExemptions_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_GameErrorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    StackTrace = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_gh_GameErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_GameErrorLogs_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gh_GameErrorLogs_gh_PlaySessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "gh_PlaySessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_AdImpressions_GameId_OccurredAt",
                table: "gh_AdImpressions",
                columns: new[] { "GameId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_AdImpressions_GameId_Type_OccurredAt",
                table: "gh_AdImpressions",
                columns: new[] { "GameId", "Type", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_ExternalResourceExemptions_GameId_Domain",
                table: "gh_ExternalResourceExemptions",
                columns: new[] { "GameId", "Domain" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_ExternalResourceExemptions_GameId_Status",
                table: "gh_ExternalResourceExemptions",
                columns: new[] { "GameId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameErrorLogs_GameId_Timestamp",
                table: "gh_GameErrorLogs",
                columns: new[] { "GameId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameErrorLogs_SessionId",
                table: "gh_GameErrorLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameErrorLogs_Timestamp_Severity",
                table: "gh_GameErrorLogs",
                columns: new[] { "Timestamp", "Severity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_AdImpressions");

            migrationBuilder.DropTable(
                name: "gh_ExternalResourceExemptions");

            migrationBuilder.DropTable(
                name: "gh_GameErrorLogs");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "gh_UserContents");

            migrationBuilder.DropColumn(
                name: "LevelEvents",
                table: "gh_PlaytestRecordings");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "DailyPlayingUsers",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "GameplayStartedCount",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "LoadingStartedCount",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "PageViews",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "gh_GameMetricSnapshots");
        }
    }
}
