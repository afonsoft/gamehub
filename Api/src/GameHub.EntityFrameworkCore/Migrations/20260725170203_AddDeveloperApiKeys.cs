using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveloperApiKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "gh_DeveloperTeams",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "gh_DeveloperProfiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_DeveloperTeams_ApiKey",
                table: "gh_DeveloperTeams",
                column: "ApiKey");

            migrationBuilder.CreateIndex(
                name: "IX_gh_DeveloperProfiles_ApiKey",
                table: "gh_DeveloperProfiles",
                column: "ApiKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_gh_DeveloperTeams_ApiKey",
                table: "gh_DeveloperTeams");

            migrationBuilder.DropIndex(
                name: "IX_gh_DeveloperProfiles_ApiKey",
                table: "gh_DeveloperProfiles");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "gh_DeveloperTeams");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "gh_DeveloperProfiles");
        }
    }
}
