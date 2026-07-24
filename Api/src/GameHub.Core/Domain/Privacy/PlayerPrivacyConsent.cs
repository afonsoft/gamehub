using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using Eaf.Middleware.Authorization.Users;
using GameHub.Catalog;

namespace GameHub.Privacy
{
    /// <summary>
    /// Records that a player has consented to a game's privacy policy.
    /// </summary>
    public class PlayerPrivacyConsent : Entity<Guid>, IMayHaveTenant, IHasCreationTime
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public Guid GameId { get; set; }

        /// <summary>Version/timestamp of the accepted privacy policy.</summary>
        [StringLength(64)]
        public string PolicyVersion { get; set; }

        [Required]
        public DateTime ConsentedAt { get; set; }

        public DateTime CreationTime { get; set; }

        public virtual User User { get; set; }

        public virtual Game Game { get; set; }
    }
}
