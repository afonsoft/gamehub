using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace GameHub.Catalog
{
    /// <summary>
    /// A single like or dislike vote for a game.
    /// </summary>
    public class GameVote : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        /// <summary>
        /// Client-generated fingerprint for anonymous users.
        /// </summary>
        [StringLength(64)]
        public string DeviceId { get; set; }

        [Required]
        public GameVoteType VoteType { get; set; }

        public virtual Game Game { get; set; }
    }
}
