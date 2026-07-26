using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using GameHub.Catalog;

namespace GameHub.ArbitraryUserData
{
    /// <summary>
    /// Generic key/value JSON store scoped to a game and optionally a user or anonymous identifier.
    /// </summary>
    public class ArbitraryUserDataRecord : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public long? UserId { get; set; }

        [StringLength(128)]
        public string AnonymousIdHash { get; set; }

        [Required]
        [StringLength(128)]
        public string Key { get; set; }

        /// <summary>JSON value payload. Max 64 KB by default validation.</summary>
        public string ValueJson { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public virtual Game Game { get; set; }
    }
}
