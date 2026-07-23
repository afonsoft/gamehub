using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// File inside a stored game build package.
    /// </summary>
    public class BuildFileDto
    {
        /// <summary>File name relative to the build root.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Full storage key.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long SizeBytes { get; set; }

        /// <summary>Public URL to access the file.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Content type inferred from extension.</summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>Last modified timestamp (UTC).</summary>
        public DateTime? LastModified { get; set; }

        /// <summary>True when this file is the entry point index.html.</summary>
        public bool IsIndexHtml { get; set; }
    }
}
