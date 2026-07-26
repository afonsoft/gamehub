using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Playtesting
{
    /// <summary>
    /// A playtest session requested for a game build by a developer or QA.
    /// </summary>
    public class PlaytestSession : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public long RequestedByUserId { get; set; }

        public PlaytestSessionStatus Status { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        [StringLength(2048)]
        public string RecordingUrl { get; set; }

        /// <summary>Whether this playtest is available for anonymous discovery on the home page.</summary>
        public bool IsDiscovery { get; set; }

        /// <summary>Probability (0 to 1) that this playtest appears in the Mystery Tile.</summary>
        public double DisplayProbability { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
