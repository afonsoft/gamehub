using Abp.Zero.EntityFrameworkCore;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.Chat;
using Eaf.Middleware.Core.Cache;
using Eaf.Middleware.Friendships;
using Eaf.Middleware.MultiTenancy;
using Eaf.Middleware.Storage;
using GameHub;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Configuration;
using GameHub.Moderation;
using Abp.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using System;
using System.Linq;

namespace GameHub.EntityFrameworkCore
{
    public class GameHubDbContext : AbpZeroDbContext<Tenant, Role, User, GameHubDbContext>
    {
        private static bool _created = false;
        private static readonly object _migrateLock = new object();
        public static bool SkipMigrate { get; set; } = false;

        public GameHubDbContext(DbContextOptions<GameHubDbContext> options) : base(options)
        {
            MigrateDatabase(Database);
        }

        private static void MigrateDatabase(DatabaseFacade database)
        {
            if (!_created)
            {
                lock (_migrateLock)
                {
                    if (!_created)
                    {
                        try
                        {
                            _created = true;
                            if (!SkipMigrate)
                            {
                                LogHelper.Logger.Trace("Database Migrate started...");
                                database.Migrate();
                            }
                        }
                        catch (Exception ex)
                        {
                            _created = false;
                            LogHelper.Logger.Warn("Database Migrate started Error ...", ex);
                        }
                    }
                }
            }
        }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }
        public virtual DbSet<Friendship> Friendships { get; set; }
        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<EafCache> EafCaches { get; set; }
        public virtual DbSet<TenantAddress> TenantAddress { get; set; }

        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<GameBuild> GameBuilds { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<GameCategory> GameCategories { get; set; }
        public virtual DbSet<GameTag> GameTags { get; set; }
        public virtual DbSet<GamePlacement> GamePlacements { get; set; }
        public virtual DbSet<DeveloperProfile> DeveloperProfiles { get; set; }
        public virtual DbSet<PlaySession> PlaySessions { get; set; }
        public virtual DbSet<GameplayEvent> GameplayEvents { get; set; }
        public virtual DbSet<GameMetricSnapshot> GameMetricSnapshots { get; set; }
        public virtual DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }
        public virtual DbSet<CloudSave> CloudSaves { get; set; }
        public virtual DbSet<ModerationReview> ModerationReviews { get; set; }
        public virtual DbSet<UserReport> UserReports { get; set; }
        public virtual DbSet<GameVote> GameVotes { get; set; }
        public virtual DbSet<FeatureFlag> FeatureFlags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var isSqlServer = optionsBuilder.Options.Extensions
                .Any(e => e.GetType().FullName?.Contains("SqlServer") == true);

            if (isSqlServer)
            {
                optionsBuilder.ConfigureWarnings(w => w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
            }
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.Name });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<EafCache>(b =>
            {
                b.HasIndex(e => new { e.Id });
                b.HasIndex(e => new { e.ExpiresAtTime });
            });

            modelBuilder.Entity<BinaryObject>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            if (Database.IsSqlServer())
            {
                modelBuilder.Entity<Abp.Auditing.AuditLog>(b =>
                {
                    b.Property(e => e.Parameters).HasColumnType("nvarchar(max)");
                });
            }
            else if (Database.IsNpgsql())
            {
                modelBuilder.Entity<Abp.Auditing.AuditLog>(b =>
                {
                    b.Property(e => e.Parameters).HasColumnType("text");
                });
            }

            modelBuilder.ConfigureGameHub();
        }
    }
}