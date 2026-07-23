using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using Eaf.Middleware.Authorization.Users;
using GameHub.Catalog;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay
{
    /// <summary>
    /// Stores player save data for a game. Logged-in users are identified by <see cref="UserId"/>.
    /// Anonymous players keep saves locally and do not create rows here.
    /// </summary>
    public class CloudSave : Entity<Guid>, IMayHaveTenant, IHasCreationTime, IHasModificationTime
    {
        public int? TenantId { get; set; }

        public Guid GameId { get; set; }

        public long? UserId { get; set; }

        /// <summary>Short opaque identifier used as fallback when the user is not authenticated.</summary>
        [StringLength(128)]
        public string DeviceIdHash { get; set; }

        /// <summary>Compressed or raw JSON payload. Limited to 1 MB after gzip by the app service.</summary>
        [StringLength(4000000)]
        public string Data { get; set; }

        /// <summary>Original uncompressed size in bytes, for monitoring.</summary>
        public long UncompressedSize { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public virtual Game Game { get; set; }

        public virtual User User { get; set; }
    }
}
