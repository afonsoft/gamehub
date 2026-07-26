using System.Collections.Generic;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Resultado agregado de métricas de um jogo.
/// </summary>
public class GameMetricsResult
{
    /// <summary>Total plays.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Total unique players.</summary>
    public long TotalUniquePlayers { get; set; }

    /// <summary>Average session duration in seconds.</summary>
    public double AverageDurationSeconds { get; set; }

    /// <summary>Game loading finished events.</summary>
    public long LoadingFinishedCount { get; set; }

    /// <summary>Gameplay started events.</summary>
    public long GameplayStartedCount { get; set; }

    /// <summary>Game page view events.</summary>
    public long PageViewCount { get; set; }

    /// <summary>Conversion events reported by the SDK.</summary>
    public long ConversionCount { get; set; }

    /// <summary>Error events.</summary>
    public long ErrorCount { get; set; }

    /// <summary>Completed commercial breaks.</summary>
    public long CommercialBreakCount { get; set; }

    /// <summary>Completed rewarded breaks.</summary>
    public long RewardedBreakCount { get; set; }

    /// <summary>Daily breakdown.</summary>
    public List<GameMetricsDailyItemDto> Daily { get; set; } = new();
}
