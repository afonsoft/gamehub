using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using Eaf.Middleware.Authorization.Users;
using GameHub.Catalog;

namespace GameHub.Player
{
    /// <summary>
    /// A recently played game tracked per player (authenticated or anonymous).
    /// </summary>
    public class PlayerRecentGame : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public PlayerRecentGame() { }

        public PlayerRecentGame(Guid id, Guid gameId, long? userId)
        {
            Id = id;
            GameId = gameId;
            UserId = userId;
            LastPlayedAt = DateTime.UtcNow;
            TotalSessions = 1;
        }

        [Required]
        public Guid GameId { get; set; }

        public long? UserId { get; set; }

        public DateTime LastPlayedAt { get; set; }

        public long TotalSessions { get; set; }

        public virtual Game Game { get; set; }

        public virtual User User { get; set; }
    }
}
