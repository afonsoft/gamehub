using System.Collections.Generic;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Result of validating a game build package.
    /// </summary>
    public class ValidationSummaryDto
    {
        /// <summary>Whether the package passed all blocking checks.</summary>
        public bool IsValid { get; set; }

        /// <summary>Blocking validation errors.</summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>Non-blocking warnings.</summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>Package size in bytes.</summary>
        public long PackageSizeBytes { get; set; }

        /// <summary>SHA-256 hash of the package.</summary>
        public string HashSha256 { get; set; } = string.Empty;

        /// <summary>Whether the package contains an index.html entry.</summary>
        public bool HasIndexHtml { get; set; }

        /// <summary>Relative path of the index.html entry inside the ZIP.</summary>
        public string IndexHtmlPath { get; set; } = string.Empty;
    }
}
