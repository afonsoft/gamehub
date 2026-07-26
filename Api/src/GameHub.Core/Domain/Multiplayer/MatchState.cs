using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using GameHub.Catalog;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Represents a lightweight online match for a multiplayer-enabled game.
    /// </summary>
    public class MatchState : FullAuditedAggregateRoot<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        [StringLength(16)]
        public string RoomCode { get; set; }

        [StringLength(64)]
        public string Mode { get; set; }

        [Required]
        public MatchStatus Status { get; set; }

        [Required]
        public int MaxPlayers { get; set; }

        /// <summary>Arbitrary JSON payload representing the shared match state.</summary>
        public string PayloadJson { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [StringLength(64)]
        public string Region { get; set; }

        public bool IsRanked { get; set; }

        public Guid? RankedSeasonId { get; set; }

        public int? AverageLatencyMs { get; set; }

        public DateTime? CompletedAt { get; set; }

        public virtual Game Game { get; set; }

        public virtual ICollection<MatchParticipant> Participants { get; protected set; } = new List<MatchParticipant>();

        public MatchState() { }

        public MatchState(Guid id, Guid gameId, string roomCode, string mode, int maxPlayers)
        {
            Id = id;
            GameId = gameId;
            RoomCode = roomCode;
            Mode = mode;
            MaxPlayers = maxPlayers;
            Status = MatchStatus.Waiting;
        }

        public bool CanJoin()
        {
            return Status == MatchStatus.Waiting && Participants.Count < MaxPlayers;
        }

        public void Start()
        {
            if (Status == MatchStatus.Waiting)
            {
                Status = MatchStatus.InProgress;
                StartedAt = DateTime.UtcNow;
            }
        }

        public void End()
        {
            if (Status != MatchStatus.Ended)
            {
                Status = MatchStatus.Ended;
                EndedAt = DateTime.UtcNow;
            }
        }
    }
}
