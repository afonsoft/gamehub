using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Gameplay.Dto;

/// <summary>
/// Input for leaderboard queries.
/// </summary>
public class GetLeaderboardInput
{
    /// <summary>Game identifier.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Number of top entries to return.</summary>
    [Range(1, 100)]
    public int Take { get; set; } = 50;
}
