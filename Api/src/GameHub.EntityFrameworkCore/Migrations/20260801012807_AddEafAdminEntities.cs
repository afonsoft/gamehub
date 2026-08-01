using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameHub.Migrations
{
    /// <inheritdoc />
    public partial class AddEafAdminEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDateUtc",
                table: "AbpTenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BiannualPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPaymentPeriodType",
                table: "AbpEditions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpEditions",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExpiringEditionId",
                table: "AbpEditions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PermanentPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuarterlyPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrialDayCount",
                table: "AbpEditions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WaitingDayAfterExpire",
                table: "AbpEditions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyPrice",
                table: "AbpEditions",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EafMassNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    Subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<byte>(type: "smallint", nullable: false),
                    TargetUserIds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetRoleIds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetOrganizationUnitIds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SendToAllUsers = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EafMassNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EafSubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    EditionId = table.Column<int>(type: "integer", nullable: false),
                    EditionPaymentType = table.Column<int>(type: "integer", nullable: false),
                    PaymentPeriodType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Gateway = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExternalPaymentId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GatewayResponse = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PaymentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_EafSubscriptionPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EafUserDelegations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    SourceUserId = table.Column<long>(type: "bigint", nullable: false),
                    TargetUserId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_EafUserDelegations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EafMassNotifications_CreationTime",
                table: "EafMassNotifications",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_EafMassNotifications_TenantId_Status",
                table: "EafMassNotifications",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EafSubscriptionPayments_CreationTime",
                table: "EafSubscriptionPayments",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_EafSubscriptionPayments_TenantId_Status",
                table: "EafSubscriptionPayments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EafUserDelegations_EndTime",
                table: "EafUserDelegations",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_EafUserDelegations_StartTime",
                table: "EafUserDelegations",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_EafUserDelegations_TenantId_SourceUserId",
                table: "EafUserDelegations",
                columns: new[] { "TenantId", "SourceUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_EafUserDelegations_TenantId_TargetUserId",
                table: "EafUserDelegations",
                columns: new[] { "TenantId", "TargetUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EafMassNotifications");

            migrationBuilder.DropTable(
                name: "EafSubscriptionPayments");

            migrationBuilder.DropTable(
                name: "EafUserDelegations");

            migrationBuilder.DropColumn(
                name: "SubscriptionEndDateUtc",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "AnnualPrice",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "BiannualPrice",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "DailyPrice",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "DefaultPaymentPeriodType",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "ExpiringEditionId",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "PermanentPrice",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "QuarterlyPrice",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "TrialDayCount",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "WaitingDayAfterExpire",
                table: "AbpEditions");

            migrationBuilder.DropColumn(
                name: "WeeklyPrice",
                table: "AbpEditions");
        }
    }
}
