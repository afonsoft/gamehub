using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPoki25Phase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FlatFeeAmount",
                table: "gh_RevenueContracts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "DisplayProbability",
                table: "gh_PlaytestSessions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscovery",
                table: "gh_PlaytestSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlaytest",
                table: "gh_PlaySessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecordingConsentGiven",
                table: "gh_PlaySessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "AvgSessionDurationSeconds",
                table: "gh_GameMetricSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "FpsAcceptableSessions",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FpsTotalSessions",
                table: "gh_GameMetricSnapshots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "MedianSessionDurationSeconds",
                table: "gh_GameMetricSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OnboardingDropOffRate",
                table: "gh_GameMetricSnapshots",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "gh_PlaytestRecordings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    PlaytestSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DeviceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    ConsoleOutput = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_gh_PlaytestRecordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_PlaytestRecordings_gh_PlaytestSessions_PlaytestSessionId",
                        column: x => x.PlaytestSessionId,
                        principalTable: "gh_PlaytestSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlaytestRecordings_PlaytestSessionId",
                table: "gh_PlaytestRecordings",
                column: "PlaytestSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_PlaytestRecordings");

            migrationBuilder.DropColumn(
                name: "FlatFeeAmount",
                table: "gh_RevenueContracts");

            migrationBuilder.DropColumn(
                name: "DisplayProbability",
                table: "gh_PlaytestSessions");

            migrationBuilder.DropColumn(
                name: "IsDiscovery",
                table: "gh_PlaytestSessions");

            migrationBuilder.DropColumn(
                name: "IsPlaytest",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "RecordingConsentGiven",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "AvgSessionDurationSeconds",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "FpsAcceptableSessions",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "FpsTotalSessions",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "MedianSessionDurationSeconds",
                table: "gh_GameMetricSnapshots");

            migrationBuilder.DropColumn(
                name: "OnboardingDropOffRate",
                table: "gh_GameMetricSnapshots");
        }
    }
}
