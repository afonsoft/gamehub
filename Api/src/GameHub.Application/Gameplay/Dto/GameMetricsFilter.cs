using System;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Filtro para consulta de métricas de um jogo.
/// </summary>
public class GameMetricsFilter
{
    /// <summary>Start date (inclusive).</summary>
    public DateTime? From { get; set; }

    /// <summary>End date (inclusive).</summary>
    public DateTime? To { get; set; }

    /// <summary>Country code filter (e.g., "BR").</summary>
    public string CountryCode { get; set; }

    /// <summary>Device type filter (e.g., "desktop", "mobile").</summary>
    public string DeviceType { get; set; }
}
