using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using GameHub.Catalog;

namespace GameHub.Gameplay
{
    /// <summary>
    /// Log of an error reported by a game during a play session.
    /// </summary>
    public class GameErrorLog : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public GameErrorLog() { }

        public GameErrorLog(Guid id, Guid? sessionId, Guid gameId, string message, string severity)
        {
            Id = id;
            SessionId = sessionId;
            GameId = gameId;
            Message = message;
            Severity = severity;
            Timestamp = DateTime.UtcNow;
        }

        public Guid? SessionId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public Guid? BuildId { get; set; }

        [Required]
        [StringLength(2048)]
        public string Message { get; set; }

        [StringLength(4000)]
        public string StackTrace { get; set; }

        [StringLength(256)]
        public string Source { get; set; }

        [StringLength(32)]
        public string Severity { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public virtual PlaySession Session { get; set; }

        public virtual Game Game { get; set; }
    }
}
