using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectorQaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gh_RevenueContracts_gh_Games_GameId1",
                table: "gh_RevenueContracts");

            migrationBuilder.DropIndex(
                name: "IX_gh_RevenueContracts_GameId1",
                table: "gh_RevenueContracts");

            migrationBuilder.DropColumn(
                name: "GameId1",
                table: "gh_RevenueContracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameId1",
                table: "gh_RevenueContracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_RevenueContracts_GameId1",
                table: "gh_RevenueContracts",
                column: "GameId1");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_RevenueContracts_gh_Games_GameId1",
                table: "gh_RevenueContracts",
                column: "GameId1",
                principalTable: "gh_Games",
                principalColumn: "Id");
        }
    }
}
