using System;
using System.Collections.Generic;

namespace GameHub.Builds.Dto
{
    /// <summary>
    /// List item DTO for build validation reports.
    /// </summary>
    public class BuildValidationReportListItemDto
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public Guid GameBuildId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public bool IsValid { get; set; }

        public bool HasExternalRequests { get; set; }

        public int WarningsCount { get; set; }

        public int ErrorsCount { get; set; }

        public List<string> Warnings { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
