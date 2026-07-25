using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveloperTeamAndPlaytest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gh_DeveloperTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PrimaryContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Country = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_gh_DeveloperTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_PlaytestSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecordingUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_gh_PlaytestSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gh_DeveloperBillingProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PaymentMethodPlaceholder = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsPendingReview = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_gh_DeveloperBillingProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_DeveloperBillingProfiles_gh_DeveloperTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "gh_DeveloperTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_DeveloperTeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvitationToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_gh_DeveloperTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_DeveloperTeamMembers_gh_DeveloperTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "gh_DeveloperTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_DeveloperBillingProfiles_TeamId",
                table: "gh_DeveloperBillingProfiles",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_DeveloperTeamMembers_InvitationToken",
                table: "gh_DeveloperTeamMembers",
                column: "InvitationToken");

            migrationBuilder.CreateIndex(
                name: "IX_gh_DeveloperTeamMembers_TeamId_UserId",
                table: "gh_DeveloperTeamMembers",
                columns: new[] { "TeamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlaytestSessions_GameId_Status",
                table: "gh_PlaytestSessions",
                columns: new[] { "GameId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlaytestSessions_RequestedByUserId",
                table: "gh_PlaytestSessions",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_DeveloperBillingProfiles");

            migrationBuilder.DropTable(
                name: "gh_DeveloperTeamMembers");

            migrationBuilder.DropTable(
                name: "gh_PlaytestSessions");

            migrationBuilder.DropTable(
                name: "gh_DeveloperTeams");
        }
    }
}
