using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto
{
    /// <summary>Input for persisting a cloud save.</summary>
    public class SaveCloudSaveInput
    {
        /// <summary>Game identifier.</summary>
        [Required]
        public Guid GameId { get; set; }

        /// <summary>Anonymous device id used as fallback when not logged in.</summary>
        public string DeviceId { get; set; }

        /// <summary>Save payload. The application layer validates the size limit.</summary>
        [Required]
        [StringLength(4000000)]
        public string Data { get; set; }
    }
}
