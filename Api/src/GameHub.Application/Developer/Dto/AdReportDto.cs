using System;
using System.Collections.Generic;

namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Ad impression and revenue report for a developer's games.
    /// </summary>
    public class AdReportDto
    {
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public long TotalImpressions { get; set; }

        public decimal TotalEarnings { get; set; }

        public decimal AverageCpm { get; set; }

        public List<AdReportItemDto> Items { get; set; } = new();
    }

    public class AdReportItemDto
    {
        public Guid GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        public string CountryCode { get; set; } = string.Empty;

        public string DeviceType { get; set; } = string.Empty;

        public long Impressions { get; set; }

        public decimal Earnings { get; set; }

        public decimal Cpm { get; set; }
    }
}
