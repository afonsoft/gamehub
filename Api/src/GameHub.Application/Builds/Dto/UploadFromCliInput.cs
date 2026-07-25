using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Builds.Dto
{
    /// <summary>Input for uploading a game build from the GameHub CLI.</summary>
    public class UploadFromCliInput
    {
        /// <summary>API key tied to the developer profile or team.</summary>
        [Required]
        [StringLength(128)]
        public string ApiKey { get; set; }

        /// <summary>Slug of the game to which the build belongs.</summary>
        [Required]
        [StringLength(128)]
        public string GameSlug { get; set; }

        /// <summary>Build package as a base64-encoded byte array.</summary>
        [Required]
        public byte[] Package { get; set; }

        /// <summary>Optional version override.</summary>
        [StringLength(64)]
        public string Version { get; set; }
    }
}
