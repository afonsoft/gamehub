using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gh_DeveloperProfiles_AbpUsers_UserId1",
                table: "gh_DeveloperProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_GameCategories_gh_Categories_CategoryId1",
                table: "gh_GameCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_GameTags_gh_Tags_TagId1",
                table: "gh_GameTags");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_LeaderboardEntries_AbpUsers_UserId1",
                table: "gh_LeaderboardEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_ModerationReviews_AbpUsers_ReviewerId",
                table: "gh_ModerationReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_PlaySessions_AbpUsers_UserId1",
                table: "gh_PlaySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_UserReports_AbpUsers_UserId1",
                table: "gh_UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_gh_UserReports_gh_ModerationReviews_ModerationReviewId1",
                table: "gh_UserReports");

            migrationBuilder.DropIndex(
                name: "IX_gh_UserReports_ModerationReviewId1",
                table: "gh_UserReports");

            migrationBuilder.DropIndex(
                name: "IX_gh_UserReports_UserId1",
                table: "gh_UserReports");

            migrationBuilder.DropIndex(
                name: "IX_gh_PlaySessions_UserId1",
                table: "gh_PlaySessions");

            migrationBuilder.DropIndex(
                name: "IX_gh_ModerationReviews_ReviewerId",
                table: "gh_ModerationReviews");

            migrationBuilder.DropIndex(
                name: "IX_gh_LeaderboardEntries_UserId1",
                table: "gh_LeaderboardEntries");

            migrationBuilder.DropIndex(
                name: "IX_gh_GameTags_TagId1",
                table: "gh_GameTags");

            migrationBuilder.DropIndex(
                name: "IX_gh_GameCategories_CategoryId1",
                table: "gh_GameCategories");

            migrationBuilder.DropIndex(
                name: "IX_gh_DeveloperProfiles_UserId1",
                table: "gh_DeveloperProfiles");

            migrationBuilder.DropColumn(
                name: "ModerationReviewId1",
                table: "gh_UserReports");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "gh_UserReports");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "gh_PlaySessions");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "gh_ModerationReviews");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "gh_LeaderboardEntries");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "gh_GameTags");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "gh_GameCategories");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "gh_DeveloperProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModerationReviewId1",
                table: "gh_UserReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "gh_UserReports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "gh_PlaySessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReviewerId",
                table: "gh_ModerationReviews",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "gh_LeaderboardEntries",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TagId1",
                table: "gh_GameTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId1",
                table: "gh_GameCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "gh_DeveloperProfiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_gh_UserReports_ModerationReviewId1",
                table: "gh_UserReports",
                column: "ModerationReviewId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_UserReports_UserId1",
                table: "gh_UserReports",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_PlaySessions_UserId1",
                table: "gh_PlaySessions",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_ModerationReviews_ReviewerId",
                table: "gh_ModerationReviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_gh_LeaderboardEntries_UserId1",
                table: "gh_LeaderboardEntries",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameTags_TagId1",
                table: "gh_GameTags",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_GameCategories_CategoryId1",
                table: "gh_GameCategories",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_gh_DeveloperProfiles_UserId1",
                table: "gh_DeveloperProfiles",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_DeveloperProfiles_AbpUsers_UserId1",
                table: "gh_DeveloperProfiles",
                column: "UserId1",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_GameCategories_gh_Categories_CategoryId1",
                table: "gh_GameCategories",
                column: "CategoryId1",
                principalTable: "gh_Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_GameTags_gh_Tags_TagId1",
                table: "gh_GameTags",
                column: "TagId1",
                principalTable: "gh_Tags",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_LeaderboardEntries_AbpUsers_UserId1",
                table: "gh_LeaderboardEntries",
                column: "UserId1",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_ModerationReviews_AbpUsers_ReviewerId",
                table: "gh_ModerationReviews",
                column: "ReviewerId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_PlaySessions_AbpUsers_UserId1",
                table: "gh_PlaySessions",
                column: "UserId1",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_UserReports_AbpUsers_UserId1",
                table: "gh_UserReports",
                column: "UserId1",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gh_UserReports_gh_ModerationReviews_ModerationReviewId1",
                table: "gh_UserReports",
                column: "ModerationReviewId1",
                principalTable: "gh_ModerationReviews",
                principalColumn: "Id");
        }
    }
}
