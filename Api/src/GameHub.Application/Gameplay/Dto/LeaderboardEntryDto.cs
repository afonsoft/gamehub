using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
namespace GameHub.Gameplay.Dto;

/// <summary>
/// Single leaderboard entry.
/// </summary>
public class LeaderboardEntryDto
{
    /// <summary>Rank position (1-based).</summary>
    public int Rank { get; set; }

    /// <summary>User identifier.</summary>
    public long UserId { get; set; }

    /// <summary>Player display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Best score achieved.</summary>
    public long Score { get; set; }

    /// <summary>UTC timestamp of the score update.</summary>
    public DateTime UpdatedAt { get; set; }
}
