using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Moderation
{
    /// <summary>
    /// User-generated content such as comments or reviews, subject to profanity filtering and moderation.
    /// </summary>
    public class UserContent : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public long? UserId { get; set; }

        [Required]
        public UserContentType ContentType { get; set; }

        [Required]
        [StringLength(4000)]
        public string Text { get; set; }

        public bool IsApproved { get; set; }

        public bool RequiresModeration { get; set; }

        [StringLength(1000)]
        public string ModerationReason { get; set; }

        public virtual GameHub.Catalog.Game Game { get; set; }
    }

    public enum UserContentType
    {
        Comment = 0,
        Review = 1
    }
}
