using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaySessionClientRequestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientRequestId",
                table: "gh_PlaySessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlaySessions_GameId_ClientRequestId",
                table: "gh_PlaySessions",
                columns: new[] { "GameId", "ClientRequestId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_gh_PlaySessions_GameId_ClientRequestId",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "gh_PlaySessions");
        }
    }
}
