using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Input for a developer to approve one of their uploaded builds.
    /// </summary>
    public class DeveloperApproveBuildInput
    {
        /// <summary>Build to approve.</summary>
        [Required]
        public Guid GameBuildId { get; set; }

        /// <summary>Optional approval notes.</summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
