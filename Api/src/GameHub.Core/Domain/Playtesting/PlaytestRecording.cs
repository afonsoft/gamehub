using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Playtesting
{
    /// <summary>
    /// A recording captured during a playtest session.
    /// </summary>
    public class PlaytestRecording : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid PlaytestSessionId { get; set; }

        /// <summary>Public URL of the recording.</summary>
        [StringLength(2048)]
        public string Url { get; set; }

        /// <summary>Duration in seconds.</summary>
        public int DurationSeconds { get; set; }

        /// <summary>Device type used during the playtest.</summary>
        [StringLength(32)]
        public string DeviceType { get; set; }

        /// <summary>ISO country code of the player.</summary>
        [StringLength(2)]
        public string CountryCode { get; set; }

        /// <summary>Console or debug output captured during the session.</summary>
        [StringLength(4000)]
        public string ConsoleOutput { get; set; }

        /// <summary>Notes added by QA or the developer.</summary>
        [StringLength(4000)]
        public string Notes { get; set; }

        /// <summary>JSON array of level events (start, death, restart, complete) used for difficulty balancing.</summary>
        [StringLength(16000)]
        public string LevelEvents { get; set; }

        public virtual PlaytestSession PlaytestSession { get; set; }
    }
}
