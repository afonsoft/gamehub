using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto
{
    /// <summary>
    /// Input for capturing an error reported by a game.
    /// </summary>
    public class CaptureGameErrorInput
    {
        [Required]
        public Guid SessionId { get; set; }

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
        public string Severity { get; set; } = "Error";
    }
}
