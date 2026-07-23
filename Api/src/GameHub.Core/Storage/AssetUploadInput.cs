using System;
using System.IO;

namespace GameHub.Storage
{
    /// <summary>
    /// Input for storing a generic game asset (image, icon, etc.) outside of a build package.
    /// </summary>
    public class AssetUploadInput
    {
        /// <summary>Game identifier used as part of the storage prefix.</summary>
        public Guid GameId { get; set; }

        /// <summary>Asset kind: thumbnails, heroes, etc.</summary>
        public string AssetKind { get; set; } = string.Empty;

        /// <summary>Original file name with extension.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>MIME content type.</summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>File content stream.</summary>
        public Stream Content { get; set; }
    }
}
