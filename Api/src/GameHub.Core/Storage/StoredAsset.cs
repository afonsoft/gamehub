using System;

namespace GameHub.Storage
{
    public class StoredAsset
    {
        /// <summary>Public URL of the original package (ZIP).</summary>
        public string Url { get; set; }

        /// <summary>Public base URL (prefix) where the extracted build files are accessible.</summary>
        public string PublicBaseUrl { get; set; }

        public string Key { get; set; }
        public string ETag { get; set; }
        public long SizeBytes { get; set; }
    }
}
