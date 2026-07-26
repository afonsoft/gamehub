using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class Poki27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPlayersPerMatch",
                table: "gh_Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsMultiplayer",
                table: "gh_Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "gh_ArbitraryUserData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    AnonymousIdHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueJson = table.Column<string>(type: "character varying(65536)", maxLength: 65536, nullable: true),
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
                    table.PrimaryKey("PK_gh_ArbitraryUserData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_ArbitraryUserData_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_MatchStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Mode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_gh_MatchStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_MatchStates_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_MatchParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    AnonymousIdHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ConnectionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_gh_MatchParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_MatchParticipants_gh_MatchStates_MatchId",
                        column: x => x.MatchId,
                        principalTable: "gh_MatchStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_ArbitraryUserData_ExpiresAt",
                table: "gh_ArbitraryUserData",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_gh_ArbitraryUserData_GameId_AnonymousIdHash_Key",
                table: "gh_ArbitraryUserData",
                columns: new[] { "GameId", "AnonymousIdHash", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_ArbitraryUserData_GameId_UserId_AnonymousIdHash_Key",
                table: "gh_ArbitraryUserData",
                columns: new[] { "GameId", "UserId", "AnonymousIdHash", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchParticipants_ConnectionId",
                table: "gh_MatchParticipants",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchParticipants_MatchId_IsActive",
                table: "gh_MatchParticipants",
                columns: new[] { "MatchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchStates_ExpiresAt",
                table: "gh_MatchStates",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchStates_GameId_Status",
                table: "gh_MatchStates",
                columns: new[] { "GameId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_MatchStates_RoomCode",
                table: "gh_MatchStates",
                column: "RoomCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_ArbitraryUserData");

            migrationBuilder.DropTable(
                name: "gh_MatchParticipants");

            migrationBuilder.DropTable(
                name: "gh_MatchStates");

            migrationBuilder.DropColumn(
                name: "MaxPlayersPerMatch",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "SupportsMultiplayer",
                table: "gh_Games");
        }
    }
}
