using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Filtro para consulta de métricas de um jogo.
/// </summary>
public class GameMetricsFilter : PagedAndSortedResultRequestDto
{
    /// <summary>Start date (inclusive).</summary>
    public DateTime? From { get; set; }

    /// <summary>End date (inclusive).</summary>
    public DateTime? To { get; set; }

    /// <summary>Build identifier filter.</summary>
    public Guid? BuildId { get; set; }

    /// <summary>Country code filter (e.g., "BR").</summary>
    [StringLength(2)]
    public string CountryCode { get; set; }

    /// <summary>Device type filter (e.g., "desktop", "mobile").</summary>
    [StringLength(32)]
    public string DeviceType { get; set; }

    /// <summary>Traffic source filter (e.g., "poki", "organic").</summary>
    [StringLength(64)]
    public string TrafficSource { get; set; }

    /// <summary>UTM source filter.</summary>
    [StringLength(64)]
    public string UtmSource { get; set; }

    /// <summary>UTM medium filter.</summary>
    [StringLength(64)]
    public string UtmMedium { get; set; }

    /// <summary>UTM campaign filter.</summary>
    [StringLength(128)]
    public string UtmCampaign { get; set; }

    /// <summary>Include playtest sessions. Defaults to false.</summary>
    public bool IsPlaytest { get; set; }
}
