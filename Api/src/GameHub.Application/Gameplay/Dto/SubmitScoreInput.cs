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
/// Input to submit a score to the leaderboard.
/// </summary>
public class SubmitScoreInput
{
    /// <summary>Game identifier.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Score value (higher is better).</summary>
    [Required]
    [Range(0, long.MaxValue)]
    public long Score { get; set; }

    /// <summary>Optional metadata JSON (level, combo, etc.).</summary>
    [StringLength(4096)]
    public string MetadataJson { get; set; }
}
