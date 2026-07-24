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
    /// A favorite game saved by a player (authenticated or anonymous).
    /// </summary>
    public class PlayerFavorite : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public PlayerFavorite() { }

        public PlayerFavorite(Guid id, Guid gameId, long? userId)
        {
            Id = id;
            GameId = gameId;
            UserId = userId;
        }

        [Required]
        public Guid GameId { get; set; }

        public long? UserId { get; set; }

        public virtual Game Game { get; set; }

        public virtual User User { get; set; }
    }
}
