using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Admin.Dto;

/// <summary>
/// Input to suspend a live game.
/// </summary>
public class SuspendGameInput
{
    /// <summary>Game to suspend.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Reason for suspension (visible to developer).</summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
