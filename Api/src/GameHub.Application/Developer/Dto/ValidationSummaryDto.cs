using System.Collections.Generic;

namespace GameHub.Developer.Dto
{
    public class ValidationSummaryDto
    {
        public bool IsValid { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

        public long SizeBytes { get; set; }

        public string HashSha256 { get; set; } = string.Empty;

        public bool HasIndexHtml { get; set; }

        /// <summary>Relative path of the index.html entry inside the ZIP.</summary>
        public string IndexHtmlPath { get; set; } = string.Empty;
    }
}
