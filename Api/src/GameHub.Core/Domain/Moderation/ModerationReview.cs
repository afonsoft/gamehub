using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using Eaf.Middleware.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Moderation
{
    public class ModerationReview : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public ModerationReview() { }

        public Guid GameId { get; set; }
        public Guid GameBuildId { get; set; }
        public long? ReviewerUserId { get; set; }
        [Required]
        public ModerationReviewStatus Status { get; set; }
        public ModerationDecision? Decision { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual Game Game { get; set; }
        public virtual GameBuild GameBuild { get; set; }
        public virtual User Reviewer { get; set; }
        public virtual ICollection<UserReport> UserReports { get; protected set; } = new List<UserReport>();
    }
}