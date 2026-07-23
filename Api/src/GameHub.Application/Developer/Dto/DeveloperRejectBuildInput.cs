using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Input for a developer to reject one of their uploaded builds.
    /// </summary>
    public class DeveloperRejectBuildInput
    {
        /// <summary>Build to reject.</summary>
        [Required]
        public Guid GameBuildId { get; set; }

        /// <summary>Rejection reason (required).</summary>
        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;
    }
}
