using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPokiCloudSaveAndControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ControlScheme",
                table: "gh_Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CutscenesSkippable",
                table: "gh_Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DefaultLanguage",
                table: "gh_Games",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedLanguages",
                table: "gh_Games",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsCloudSaves",
                table: "gh_Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "gh_PlayerPrivacyConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ConsentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_PlayerPrivacyConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_PlayerPrivacyConsents_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gh_PlayerPrivacyConsents_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerPrivacyConsents_GameId_UserId",
                table: "gh_PlayerPrivacyConsents",
                columns: new[] { "GameId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlayerPrivacyConsents_UserId",
                table: "gh_PlayerPrivacyConsents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_PlayerPrivacyConsents");

            migrationBuilder.DropColumn(
                name: "ControlScheme",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "CutscenesSkippable",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "SupportedLanguages",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "SupportsCloudSaves",
                table: "gh_Games");
        }
    }
}
