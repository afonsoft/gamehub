using System;
using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Result of the error scanner aggregation.
    /// </summary>
    public class ErrorScannerResultDto
    {
        public Guid? GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long TotalErrors { get; set; }

        public List<ErrorScannerItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Aggregated error group.
    /// </summary>
    public class ErrorScannerItemDto
    {
        public string Message { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public long Count { get; set; }

        public DateTime LastOccurredAt { get; set; }

        public List<string> Samples { get; set; } = new();
    }
}
