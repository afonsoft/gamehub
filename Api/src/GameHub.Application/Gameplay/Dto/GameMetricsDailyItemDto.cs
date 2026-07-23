using System;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Métricas diárias de um jogo.
/// </summary>
public class GameMetricsDailyItemDto
{
    /// <summary>Date of the metrics.</summary>
    public DateTime Date { get; set; }

    /// <summary>Total plays.</summary>
    public long Plays { get; set; }

    /// <summary>Unique players.</summary>
    public long UniquePlayers { get; set; }

    /// <summary>Average session duration in seconds.</summary>
    public double AvgDurationSeconds { get; set; }

    /// <summary>Game loading finished events.</summary>
    public long LoadingFinishedCount { get; set; }

    /// <summary>Error events.</summary>
    public long ErrorCount { get; set; }

    /// <summary>Completed commercial breaks.</summary>
    public long CommercialBreakCount { get; set; }

    /// <summary>Completed rewarded breaks.</summary>
    public long RewardedBreakCount { get; set; }
}
