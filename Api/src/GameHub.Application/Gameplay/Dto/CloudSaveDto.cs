using System;

namespace GameHub.Gameplay.Dto
{
    /// <summary>Cloud save data for a player.</summary>
    public class CloudSaveDto
    {
        /// <summary>Game identifier.</summary>
        public Guid GameId { get; set; }

        /// <summary>Save payload (JSON).</summary>
        public string Data { get; set; }

        /// <summary>UTC timestamp of the last update.</summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>Whether the save was persisted successfully.</summary>
        public bool Saved { get; set; } = true;

        /// <summary>Friendly message when persistence fails or is local-only.</summary>
        public string Message { get; set; }
    }
}
