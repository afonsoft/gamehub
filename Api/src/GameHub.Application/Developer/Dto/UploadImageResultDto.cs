using System;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Result of uploading a game image asset.
    /// </summary>
    public class UploadImageResultDto
    {
        /// <summary>Public URL of the uploaded image.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Storage key.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long SizeBytes { get; set; }
    }
}
