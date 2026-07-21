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
    public class UserReport : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public UserReport() { }

        public Guid GameId { get; set; }
        public long? UserId { get; set; }
        public Guid? ModerationReviewId { get; set; }
        [Required]
        [StringLength(128)]
        public string Reason { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        [Required]
        public UserReportStatus Status { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
        public virtual ModerationReview ModerationReview { get; set; }
    }
}