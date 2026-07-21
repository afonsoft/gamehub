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
/// Input to start a new play session.
/// </summary>
public class StartPlaySessionInput
{
    /// <summary>Game to play.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Player device type: "Desktop", "Mobile", "Tablet".</summary>
    [Required]
    [StringLength(20)]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>Browser user agent string.</summary>
    [StringLength(500)]
    public string Browser { get; set; }

    /// <summary>HTTP referrer that led to the game.</summary>
    [StringLength(500)]
    public string Referrer { get; set; }
}
