using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddEafTenantJoinRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantJoinRequests");

            migrationBuilder.CreateTable(
                name: "AbpTenantJoinRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    TenantUserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ApproverUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenantJoinRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenantJoinRequests_TenantId_Status",
                table: "AbpTenantJoinRequests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenantJoinRequests_UserId_TenantId",
                table: "AbpTenantJoinRequests",
                columns: new[] { "UserId", "TenantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbpTenantJoinRequests");

            migrationBuilder.CreateTable(
                name: "TenantJoinRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantJoinRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantJoinRequests_TenantId_Status",
                table: "TenantJoinRequests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantJoinRequests_UserId_TenantId",
                table: "TenantJoinRequests",
                columns: new[] { "UserId", "TenantId" });
        }
    }
}
