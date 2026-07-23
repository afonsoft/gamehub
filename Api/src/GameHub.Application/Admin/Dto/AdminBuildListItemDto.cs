using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Build list item for the admin uploads panel.
    /// </summary>
    public class AdminBuildListItemDto
    {
        /// <summary>Build unique identifier.</summary>
        public Guid Id { get; set; }

        /// <summary>Parent game identifier.</summary>
        public Guid GameId { get; set; }

        /// <summary>Game title.</summary>
        public string GameTitle { get; set; } = string.Empty;

        /// <summary>Developer display name.</summary>
        public string DeveloperName { get; set; } = string.Empty;

        /// <summary>Build version string.</summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>Sequential build number.</summary>
        public int BuildNumber { get; set; }

        /// <summary>Build status.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Package size in bytes.</summary>
        public long SizeBytes { get; set; }

        /// <summary>SHA-256 hash of the package.</summary>
        public string HashSha256 { get; set; } = string.Empty;

        /// <summary>UTC upload timestamp.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>UTC publication timestamp, if published.</summary>
        public DateTime? PublishedAt { get; set; }
    }
}
