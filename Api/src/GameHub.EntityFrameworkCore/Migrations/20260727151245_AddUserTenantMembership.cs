using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTenantMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientMessageId",
                table: "EafChatMessages",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextType",
                table: "EafChatMessages",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "EafChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "EafChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MatchId",
                table: "EafChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gh_UserTenantMemberships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    TenantUserId = table.Column<long>(type: "bigint", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gh_UserTenantMemberships", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gh_UserTenantMemberships_TenantUserId",
                table: "gh_UserTenantMemberships",
                column: "TenantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_gh_UserTenantMemberships_UserId_IsDefault",
                table: "gh_UserTenantMemberships",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_gh_UserTenantMemberships_UserId_TenantId",
                table: "gh_UserTenantMemberships",
                columns: new[] { "UserId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gh_UserTenantMemberships");

            migrationBuilder.DropColumn(
                name: "ClientMessageId",
                table: "EafChatMessages");

            migrationBuilder.DropColumn(
                name: "ContextType",
                table: "EafChatMessages");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "EafChatMessages");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "EafChatMessages");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "EafChatMessages");
        }
    }
}
