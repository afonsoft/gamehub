using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Result of validating SEO fields for a game.
    /// </summary>
    public class ValidateSeoResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Warnings { get; set; } = new();
        public string SuggestedDescription { get; set; } = string.Empty;
        public string SeoDescription { get; set; } = string.Empty;
    }
}
