using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailsInspectorChecklistPreviewToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnimatedThumbnailUrl",
                table: "gh_Games",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AspectRatio",
                table: "gh_Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThumbnailStatus",
                table: "gh_Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "gh_InspectorChecklistAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Answer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_InspectorChecklistAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_InspectorChecklistAnswers_gh_InspectorSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "gh_InspectorSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gh_PreviewTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameBuildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TokenValue = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_PreviewTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_PreviewTokens_gh_GameBuilds_GameBuildId",
                        column: x => x.GameBuildId,
                        principalTable: "gh_GameBuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_InspectorChecklistAnswers_SessionId_QuestionId",
                table: "gh_InspectorChecklistAnswers",
                columns: new[] { "SessionId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_PreviewTokens_GameBuildId",
                table: "gh_PreviewTokens",
                column: "GameBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_gh_PreviewTokens_GameId_Version",
                table: "gh_PreviewTokens",
                columns: new[] { "GameId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_PreviewTokens_TokenValue",
                table: "gh_PreviewTokens",
                column: "TokenValue",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_InspectorChecklistAnswers");

            migrationBuilder.DropTable(
                name: "gh_PreviewTokens");

            migrationBuilder.DropColumn(
                name: "AnimatedThumbnailUrl",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "AspectRatio",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "ThumbnailStatus",
                table: "gh_Games");
        }
    }
}
