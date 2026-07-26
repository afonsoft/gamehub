using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;

namespace GameHub.Multiplayer
{
    public class PlayerRating : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Required] public Guid GameId { get; set; }
        [Required] public Guid SeasonId { get; set; }
        [Required] public long UserId { get; set; }
        [Required, StringLength(64)] public string Mode { get; set; }
        public int Rating { get; set; } = 1000;
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public DateTime? LastPlayedAt { get; set; }

        public void ApplyResult(int result, int expectedResult, int kFactor = 32)
        {
            Rating = Math.Max(0, Rating + kFactor * (result - expectedResult));
            GamesPlayed++;
            if (result > expectedResult) Wins++;
            else if (result < expectedResult) Losses++;
            else Draws++;
            LastPlayedAt = DateTime.UtcNow;
        }
    }
}
