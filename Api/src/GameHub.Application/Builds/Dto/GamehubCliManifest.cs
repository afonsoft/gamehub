using System.ComponentModel.DataAnnotations;

namespace GameHub.Builds.Dto
{
    /// <summary>
    /// Contract for the <c>gamehub.json</c> manifest used by the GameHub CLI and CI integrations.
    /// </summary>
    public class GamehubCliManifest
    {
        /// <summary>API key tied to the developer profile or team.</summary>
        [Required]
        [StringLength(128)]
        public string ApiKey { get; set; }

        /// <summary>Slug of the game to which the build belongs.</summary>
        [Required]
        [StringLength(128)]
        public string GameSlug { get; set; }

        /// <summary>Optional version override. When omitted the backend generates 1.0.N.</summary>
        [StringLength(64)]
        public string Version { get; set; }

        /// <summary>Relative path to the build package file (zip).</summary>
        [StringLength(512)]
        public string PackagePath { get; set; } = "dist.zip";
    }
}
