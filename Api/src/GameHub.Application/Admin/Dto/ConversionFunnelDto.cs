using System;
using System.Collections.Generic;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Conversion funnel from page view to gameplay start.
    /// </summary>
    public class ConversionFunnelDto
    {
        public Guid GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<FunnelStageDto> Stages { get; set; } = new();
    }

    public class FunnelStageDto
    {
        public string Name { get; set; } = string.Empty;

        public long Count { get; set; }

        public double ConversionRate { get; set; }
    }
}
