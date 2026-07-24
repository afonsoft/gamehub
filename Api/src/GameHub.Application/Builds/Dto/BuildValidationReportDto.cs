using System;
using System.Collections.Generic;

namespace GameHub.Builds.Dto
{
    /// <summary>
    /// DTO for a persisted build validation report.
    /// </summary>
    public class BuildValidationReportDto
    {
        public Guid Id { get; set; }

        public Guid GameBuildId { get; set; }

        public bool IsValid { get; set; }

        public bool HasExternalRequests { get; set; }

        public List<string> Errors { get; set; } = new();

        public List<string> Warnings { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
