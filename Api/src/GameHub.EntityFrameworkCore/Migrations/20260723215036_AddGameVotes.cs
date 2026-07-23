using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddGameVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalDislikes",
                table: "gh_Games",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalLikes",
                table: "gh_Games",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "gh_GameVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VoteType = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_GameVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gh_GameVotes_gh_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "gh_Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameVotes_GameId_CreatorUserId",
                table: "gh_GameVotes",
                columns: new[] { "GameId", "CreatorUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameVotes_GameId_DeviceId",
                table: "gh_GameVotes",
                columns: new[] { "GameId", "DeviceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_GameVotes");

            migrationBuilder.DropColumn(
                name: "TotalDislikes",
                table: "gh_Games");

            migrationBuilder.DropColumn(
                name: "TotalLikes",
                table: "gh_Games");
        }
    }
}
