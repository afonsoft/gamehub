using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddMonetizationBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrafficSource",
                table: "gh_PlaySessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UtmCampaign",
                table: "gh_PlaySessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmMedium",
                table: "gh_PlaySessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmSource",
                table: "gh_PlaySessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gh_RevenueContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractType = table.Column<int>(type: "integer", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_gh_RevenueContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_RevenueContracts_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_RevenueContracts_GameId_IsActive_EffectiveDate",
                table: "gh_RevenueContracts",
                columns: new[] { "GameId", "IsActive", "EffectiveDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_RevenueContracts");

            migrationBuilder.DropColumn(
                name: "TrafficSource",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "UtmCampaign",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "UtmMedium",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "UtmSource",
                table: "gh_PlaySessions");
        }
    }
}
