using GameHub;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Configuration;
using GameHub.Inspector;
using GameHub.Moderation;
using GameHub.Monetization;
using GameHub.Player;
using Microsoft.EntityFrameworkCore;

namespace GameHub.EntityFrameworkCore
{
    public static class GameHubModelCreatingExtensions
    {
        public static void ConfigureGameHub(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "Games", GameHubConsts.DbSchema);

                b.Property(x => x.Title).IsRequired().HasMaxLength(256);
                b.Property(x => x.Slug).IsRequired().HasMaxLength(256);
                b.Property(x => x.ShortDescription).IsRequired().HasMaxLength(500);
                b.Property(x => x.Description).HasMaxLength(4000);
                b.Property(x => x.Instructions).HasMaxLength(2000);
                b.Property(x => x.Controls).HasMaxLength(4000);
                b.Property(x => x.SuggestedDescription).HasMaxLength(4000);
                b.Property(x => x.SeoDescription).HasMaxLength(500);
                b.Property(x => x.PrivacyPolicyUrl).HasMaxLength(512);
                b.Property(x => x.AgeRating).IsRequired().HasMaxLength(32);
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.Orientation).IsRequired();
                b.Property(x => x.ThumbnailUrl).HasMaxLength(512);
                b.Property(x => x.HeroImageUrl).HasMaxLength(512);
                b.Property(x => x.TotalPlays).HasDefaultValue(0L);
                b.Property(x => x.TotalLikes).HasDefaultValue(0L);
                b.Property(x => x.TotalDislikes).HasDefaultValue(0L);

                b.HasIndex(x => x.Slug).IsUnique();
                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.DeveloperProfileId);

                b.HasOne(x => x.DeveloperProfile)
                    .WithMany(x => x.Games)
                    .HasForeignKey(x => x.DeveloperProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.PublishedBuild)
                    .WithMany()
                    .HasForeignKey(x => x.PublishedBuildId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<GameBuild>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GameBuilds", GameHubConsts.DbSchema);

                b.Property(x => x.Version).IsRequired().HasMaxLength(32);
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.OriginalPackageUrl).IsRequired().HasMaxLength(1024);
                b.Property(x => x.PublicBaseUrl).HasMaxLength(1024);
                b.Property(x => x.IndexHtmlPath).HasMaxLength(512);
                b.Property(x => x.SizeBytes).IsRequired();
                b.Property(x => x.HashSha256).IsRequired().HasMaxLength(128);
                b.Property(x => x.ValidationSummary).HasMaxLength(4000);

                b.HasIndex(x => new { x.GameId, x.Version }).IsUnique();
                b.HasIndex(x => x.Status);

                b.HasOne(x => x.Game)
                    .WithMany(x => x.GameBuilds)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BuildValidationReport>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "BuildValidationReports", GameHubConsts.DbSchema);

                b.Property(x => x.GameBuildId).IsRequired();
                b.Property(x => x.IsValid).IsRequired();
                b.Property(x => x.HasExternalRequests).IsRequired();
                b.Property(x => x.ErrorsJson).HasMaxLength(4000);
                b.Property(x => x.WarningsJson).HasMaxLength(4000);
                b.Property(x => x.CreatedAt).IsRequired();

                b.HasIndex(x => x.GameBuildId);

                b.HasOne(x => x.GameBuild)
                    .WithMany()
                    .HasForeignKey(x => x.GameBuildId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "Categories", GameHubConsts.DbSchema);

                b.Property(x => x.Name).IsRequired().HasMaxLength(64);
                b.Property(x => x.Slug).IsRequired().HasMaxLength(64);
                b.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);

                b.HasIndex(x => x.Slug).IsUnique();
            });

            modelBuilder.Entity<Tag>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "Tags", GameHubConsts.DbSchema);

                b.Property(x => x.Name).IsRequired().HasMaxLength(64);
                b.Property(x => x.Slug).IsRequired().HasMaxLength(64);

                b.HasIndex(x => x.Slug).IsUnique();
            });

            modelBuilder.Entity<GameCategory>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GameCategories", GameHubConsts.DbSchema);

                b.HasKey(x => new { x.GameId, x.CategoryId });

                b.HasOne(x => x.Game)
                    .WithMany(x => x.GameCategories)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Category)
                    .WithMany(x => x.GameCategories)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GameTag>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GameTags", GameHubConsts.DbSchema);

                b.HasKey(x => new { x.GameId, x.TagId });

                b.HasOne(x => x.Game)
                    .WithMany(x => x.GameTags)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Tag)
                    .WithMany(x => x.GameTags)
                    .HasForeignKey(x => x.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GamePlacement>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GamePlacements", GameHubConsts.DbSchema);

                b.Property(x => x.PlacementType).IsRequired();
                b.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);
                b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

                b.HasIndex(x => new { x.PlacementType, x.SortOrder });

                b.HasOne(x => x.Game)
                    .WithMany(x => x.GamePlacements)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DeveloperProfile>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "DeveloperProfiles", GameHubConsts.DbSchema);

                b.Property(x => x.DisplayName).IsRequired().HasMaxLength(128);
                b.Property(x => x.LegalName).HasMaxLength(256);
                b.Property(x => x.WebsiteUrl).HasMaxLength(512);
                b.Property(x => x.SupportEmail).HasMaxLength(256);
                b.Property(x => x.Status).IsRequired();

                b.HasIndex(x => x.UserId).IsUnique();

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PlaySession>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "PlaySessions", GameHubConsts.DbSchema);

                b.Property(x => x.UserId);
                b.Property(x => x.AnonymousIdHash).HasMaxLength(128);
                b.Property(x => x.StartedAt).IsRequired();
                b.Property(x => x.DeviceType).IsRequired().HasMaxLength(32);
                b.Property(x => x.Browser).IsRequired().HasMaxLength(64);
                b.Property(x => x.CountryCode).HasMaxLength(2);
                b.Property(x => x.Referrer).HasMaxLength(1024);
                b.Property(x => x.TrafficSource).IsRequired();
                b.Property(x => x.UtmSource).HasMaxLength(128);
                b.Property(x => x.UtmMedium).HasMaxLength(128);
                b.Property(x => x.UtmCampaign).HasMaxLength(128);
                b.Property(x => x.ClientRequestId).HasMaxLength(64);
                b.Property(x => x.CommercialBreakCount).HasDefaultValue(0L);
                b.Property(x => x.RewardedBreakCount).HasDefaultValue(0L);
                b.Property(x => x.FpsAverage);
                b.Property(x => x.FpsMin);

                b.HasIndex(x => new { x.GameId, x.StartedAt });
                b.HasIndex(x => x.UserId);
                b.HasIndex(x => new { x.GameId, x.ClientRequestId });

                b.HasOne(x => x.Game)
                    .WithMany(x => x.PlaySessions)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<GameplayEvent>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GameplayEvents", GameHubConsts.DbSchema);

                b.Property(x => x.EventType).IsRequired();
                b.Property(x => x.EventName).HasMaxLength(128);
                b.Property(x => x.PayloadJson).HasMaxLength(4000);
                b.Property(x => x.OccurredAt).IsRequired();

                b.HasIndex(x => new { x.PlaySessionId, x.OccurredAt });
                b.HasIndex(x => new { x.GameId, x.OccurredAt });

                b.HasOne(x => x.PlaySession)
                    .WithMany(x => x.GameplayEvents)
                    .HasForeignKey(x => x.PlaySessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GameMetricSnapshot>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GameMetricSnapshots", GameHubConsts.DbSchema);

                b.Property(x => x.Date).IsRequired();
                b.Property(x => x.Plays).HasDefaultValue(0L);
                b.Property(x => x.UniquePlayers).HasDefaultValue(0L);
                b.Property(x => x.AvgDurationSeconds).HasDefaultValue(0.0);
                b.Property(x => x.LoadingFinishedCount).HasDefaultValue(0L);
                b.Property(x => x.ErrorCount).HasDefaultValue(0L);
                b.Property(x => x.CommercialBreakCount).HasDefaultValue(0L);
                b.Property(x => x.RewardedBreakCount).HasDefaultValue(0L);
                b.Property(x => x.AvgFps);
                b.Property(x => x.MinFps);

                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.GameId, x.Date }).IsUnique();

                b.HasOne(x => x.Game)
                    .WithMany(x => x.GameMetricSnapshots)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LeaderboardEntry>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "LeaderboardEntries", GameHubConsts.DbSchema);

                b.Property(x => x.Score).IsRequired();
                b.Property(x => x.MetadataJson).HasMaxLength(4000);
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt).IsRequired();

                b.HasIndex(x => new { x.GameId, x.UserId }).IsUnique();
                b.HasIndex(x => new { x.GameId, x.Score }).IsDescending(false, true);

                b.HasOne(x => x.Game)
                    .WithMany(x => x.LeaderboardEntries)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CloudSave>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "CloudSaves", GameHubConsts.DbSchema);

                b.Property(x => x.DeviceIdHash).HasMaxLength(128);
                b.Property(x => x.Data).HasMaxLength(4000000);

                b.HasIndex(x => new { x.GameId, x.UserId }).IsUnique();
                b.HasIndex(x => new { x.GameId, x.DeviceIdHash });

                b.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ModerationReview>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "ModerationReviews", GameHubConsts.DbSchema);

                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.Notes).HasMaxLength(2000);
                b.Property(x => x.CreationTime).HasColumnName("CreatedAt").IsRequired();
                b.Property(x => x.CompletedAt);

                b.HasIndex(x => new { x.GameId, x.Status });
                b.HasIndex(x => x.GameBuildId);
                b.HasIndex(x => x.ReviewerUserId);

                b.HasOne(x => x.Game)
                    .WithMany(x => x.ModerationReviews)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.GameBuild)
                    .WithMany(x => x.ModerationReviews)
                    .HasForeignKey(x => x.GameBuildId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Reviewer)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewerUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<GameVote>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "GameVotes", GameHubConsts.DbSchema);

                b.Property(x => x.GameId).IsRequired();
                b.Property(x => x.DeviceId).HasMaxLength(64);
                b.Property(x => x.VoteType).IsRequired();

                b.HasIndex(x => new { x.GameId, x.CreatorUserId });
                b.HasIndex(x => new { x.GameId, x.DeviceId });

                b.HasOne(x => x.Game)
                    .WithMany(x => x.GameVotes)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserReport>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "UserReports", GameHubConsts.DbSchema);

                b.Property(x => x.Reason).IsRequired().HasMaxLength(128);
                b.Property(x => x.Description).HasMaxLength(2000);
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.CreationTime).HasColumnName("CreatedAt").IsRequired();
                b.Property(x => x.ResolvedAt);

                b.HasIndex(x => x.GameId);
                b.HasIndex(x => x.Status);

                b.HasOne(x => x.Game)
                    .WithMany(x => x.UserReports)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.ModerationReview)
                    .WithMany(x => x.UserReports)
                    .HasForeignKey(x => x.ModerationReviewId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<FeatureFlag>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "FeatureFlags", GameHubConsts.DbSchema);

                b.Property(x => x.Name).IsRequired().HasMaxLength(128);
                b.Property(x => x.Description).HasMaxLength(512);
                b.Property(x => x.IsEnabled).IsRequired();

                b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            });

            modelBuilder.Entity<RevenueContract>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "RevenueContracts", GameHubConsts.DbSchema);

                b.Property(x => x.GameId).IsRequired();
                b.Property(x => x.ContractType).IsRequired();
                b.Property(x => x.EffectiveDate).IsRequired();
                b.Property(x => x.IsActive).IsRequired();

                b.HasIndex(x => new { x.GameId, x.IsActive, x.EffectiveDate });

                b.HasOne(x => x.Game)
                    .WithMany(g => g.RevenueContracts)
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlayerFavorite>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "PlayerFavorites", GameHubConsts.DbSchema);

                b.Property(x => x.GameId).IsRequired();

                b.HasIndex(x => new { x.GameId, x.UserId }).IsUnique();
                b.HasIndex(x => new { x.UserId, x.CreationTime });

                b.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlayerRecentGame>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "PlayerRecentGames", GameHubConsts.DbSchema);

                b.Property(x => x.GameId).IsRequired();
                b.Property(x => x.LastPlayedAt).IsRequired();
                b.Property(x => x.TotalSessions).HasDefaultValue(0L);

                b.HasIndex(x => new { x.GameId, x.UserId }).IsUnique();
                b.HasIndex(x => new { x.UserId, x.LastPlayedAt });

                b.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserContent>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "UserContents", GameHubConsts.DbSchema);

                b.Property(x => x.GameId).IsRequired();
                b.Property(x => x.ContentType).IsRequired();
                b.Property(x => x.Text).IsRequired().HasMaxLength(4000);
                b.Property(x => x.IsApproved).IsRequired();
                b.Property(x => x.RequiresModeration).IsRequired();
                b.Property(x => x.ModerationReason).HasMaxLength(1000);

                b.HasIndex(x => new { x.GameId, x.IsApproved, x.RequiresModeration });

                b.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InspectorSession>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "InspectorSessions", GameHubConsts.DbSchema);

                b.Property(x => x.GameId).IsRequired();
                b.Property(x => x.StartedAt).IsRequired();
                b.Property(x => x.DevicePreset).HasMaxLength(32);
                b.Property(x => x.Resolution).HasMaxLength(32);
                b.Property(x => x.Status).HasMaxLength(32);

                b.HasIndex(x => new { x.GameId, x.StartedAt });

                b.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InspectorSdkEvent>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "InspectorSdkEvents", GameHubConsts.DbSchema);

                b.Property(x => x.SessionId).IsRequired();
                b.Property(x => x.EventType).IsRequired().HasMaxLength(64);
                b.Property(x => x.Payload).HasMaxLength(2000);
                b.Property(x => x.SequenceNumber).IsRequired();
                b.Property(x => x.Timestamp).IsRequired();

                b.HasIndex(x => new { x.SessionId, x.SequenceNumber });

                b.HasOne(x => x.Session)
                    .WithMany()
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InspectorWarning>(b =>
            {
                b.ToTable(GameHubConsts.DbTablePrefix + "InspectorWarnings", GameHubConsts.DbSchema);

                b.Property(x => x.SessionId).IsRequired();
                b.Property(x => x.Category).IsRequired().HasMaxLength(64);
                b.Property(x => x.Message).IsRequired().HasMaxLength(500);
                b.Property(x => x.Severity).IsRequired().HasMaxLength(32);

                b.HasIndex(x => x.SessionId);

                b.HasOne(x => x.Session)
                    .WithMany()
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
