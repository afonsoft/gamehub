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

namespace GameHub.Builds
{
    public class GameBuild : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public GameBuild() { }

        public GameBuild(Guid id, Guid gameId, string version, int buildNumber, string originalPackageUrl, long sizeBytes, string hashSha256) : base()
        {
            Id = id;
            GameId = gameId;
            Version = version;
            BuildNumber = buildNumber;
            Status = GameBuildStatus.Uploaded;
            OriginalPackageUrl = originalPackageUrl;
            SizeBytes = sizeBytes;
            HashSha256 = hashSha256;
        }

        public void Approve()
        {
            if (Status != GameBuildStatus.Uploaded && Status != GameBuildStatus.Validated && Status != GameBuildStatus.InReview)
            {
                throw new InvalidOperationException($"Cannot approve build with status {Status}.");
            }

            Status = GameBuildStatus.Approved;
        }

        public void Reject(string reason)
        {
            if (Status == GameBuildStatus.Published)
            {
                throw new InvalidOperationException("Cannot reject a published build.");
            }

            Status = GameBuildStatus.Rejected;
            ValidationSummary = reason;
        }

        public void Publish()
        {
            if (Status != GameBuildStatus.Approved)
            {
                throw new InvalidOperationException("Only approved builds can be published.");
            }

            Status = GameBuildStatus.Published;
            PublishedTime = DateTime.UtcNow;
        }

        public Guid GameId { get; set; }
        [Required]
        [StringLength(32)]
        public string Version { get; set; }
        [Required]
        public int BuildNumber { get; set; }
        [Required]
        public GameBuildStatus Status { get; set; }
        [Required]
        [StringLength(1024)]
        public string OriginalPackageUrl { get; set; }
        [StringLength(1024)]
        public string PublicBaseUrl { get; set; }
        [StringLength(512)]
        public string IndexHtmlPath { get; set; }
        [Required]
        public long SizeBytes { get; set; }
        [Required]
        [StringLength(128)]
        public string HashSha256 { get; set; }
        [StringLength(4000)]
        public string ValidationSummary { get; set; }
        public DateTime? PublishedTime { get; set; }

        public virtual Game Game { get; set; }
        public virtual ICollection<ModerationReview> ModerationReviews { get; protected set; } = new List<ModerationReview>();
    }
}