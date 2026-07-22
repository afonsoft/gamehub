using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GameHub.Catalog
{
    public class Game : FullAuditedAggregateRoot<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public Game() { }

        public Game(Guid id, string title, string slug, string shortDescription, Guid developerProfileId) : base()
        {
            Title = title;
            Slug = slug;
            ShortDescription = shortDescription;
            Id = id;
            DeveloperProfileId = developerProfileId;
            Status = GameStatus.Draft;
            TotalPlays = 0;
            AgeRating = "Everyone";
            Orientation = GameOrientation.Both;
            SupportsDesktop = true;
            SupportsMobile = true;
            SupportsTablet = true;
        }

        public void Publish(Guid? buildId = null)
        {
            if (Status != GameStatus.InReview && Status != GameStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot publish game with status {Status}.");
            }

            var build = buildId.HasValue ? FindBuild(buildId.Value) : GameBuilds.FirstOrDefault(b => b.Status == GameBuildStatus.Approved);
            if (build == null)
            {
                throw new InvalidOperationException("No approved build found for this game.");
            }

            if (build.Status != GameBuildStatus.Approved && build.Status != GameBuildStatus.Published)
            {
                throw new InvalidOperationException("Build must be approved before publishing.");
            }

            PublishedBuildId = build.Id;
            Status = GameStatus.Published;
        }

        /// <summary>Associates the given approved/published build as the published build for this game.</summary>
        public void SetPublishedBuild(GameBuild build)
        {
            if (build == null)
                throw new ArgumentNullException(nameof(build));

            if (build.GameId != Id)
                throw new InvalidOperationException("Build does not belong to this game.");

            Publish(build.Id);
        }

        public GameBuild AddBuild(Guid buildId, string version, int buildNumber, string originalPackageUrl, long sizeBytes, string hashSha256)
        {
            var build = new GameBuild(buildId, Id, version, buildNumber, originalPackageUrl, sizeBytes, hashSha256);
            GameBuilds.Add(build);
            return build;
        }

        public GameBuild FindBuild(Guid buildId)
        {
            foreach (var build in GameBuilds)
            {
                if (build.Id == buildId)
                {
                    return build;
                }
            }

            return null;
        }

        [Required]
        [StringLength(256)]
        public string Title { get; set; }
        [Required]
        [StringLength(256)]
        public string Slug { get; set; }
        [Required]
        [StringLength(500)]
        public string ShortDescription { get; set; }
        [StringLength(4000)]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Instructions { get; set; }
        [Required]
        public GameStatus Status { get; set; }
        [Required]
        [StringLength(32)]
        public string AgeRating { get; set; }
        [Required]
        public GameOrientation Orientation { get; set; }
        [Required]
        public bool SupportsDesktop { get; set; }
        [Required]
        public bool SupportsMobile { get; set; }
        [Required]
        public bool SupportsTablet { get; set; }
        [StringLength(512)]
        public string ThumbnailUrl { get; set; }
        [StringLength(512)]
        public string HeroImageUrl { get; set; }
        public Guid DeveloperProfileId { get; set; }
        public Guid? PublishedBuildId { get; set; }
        [Required]
        public long TotalPlays { get; set; }
        public double? AverageRating { get; set; }

        public virtual DeveloperProfile DeveloperProfile { get; set; }
        public virtual GameBuild PublishedBuild { get; set; }
        public virtual ICollection<GameBuild> GameBuilds { get; protected set; } = new List<GameBuild>();
        public virtual ICollection<GameCategory> GameCategories { get; protected set; } = new List<GameCategory>();
        public virtual ICollection<GameTag> GameTags { get; protected set; } = new List<GameTag>();
        public virtual ICollection<GamePlacement> GamePlacements { get; protected set; } = new List<GamePlacement>();
        public virtual ICollection<PlaySession> PlaySessions { get; protected set; } = new List<PlaySession>();
        public virtual ICollection<GameMetricSnapshot> GameMetricSnapshots { get; protected set; } = new List<GameMetricSnapshot>();
        public virtual ICollection<LeaderboardEntry> LeaderboardEntries { get; protected set; } = new List<LeaderboardEntry>();
        public virtual ICollection<ModerationReview> ModerationReviews { get; protected set; } = new List<ModerationReview>();
        public virtual ICollection<UserReport> UserReports { get; protected set; } = new List<UserReport>();
    }
}