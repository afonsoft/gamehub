using System;

namespace GameHub.Storage
{
    /// <summary>
    /// Represents a single file stored for a game build.
    /// </summary>
    public class StoredFile
    {
        /// <summary>Full storage key (e.g., builds/{gameId}/{buildId}/index.html).</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>File name relative to the build prefix.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long SizeBytes { get; set; }

        /// <summary>Last modified timestamp (UTC).</summary>
        public DateTime? LastModified { get; set; }

        /// <summary>Public URL to access the file.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Content type inferred from the file extension.</summary>
        public string ContentType { get; set; } = string.Empty;
    }
}
