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
/// A single gameplay event from the Game SDK.
/// </summary>
public class GameplayEventInput
{
    /// <summary>Session identifier.</summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>Type of gameplay event.</summary>
    [Required]
    public GameplayEventType EventType { get; set; }

    /// <summary>Event name (e.g., "level_complete").</summary>
    [StringLength(100)]
    public string EventName { get; set; }

    /// <summary>Arbitrary JSON payload.</summary>
    [StringLength(4096)]
    public string PayloadJson { get; set; }
}

/// <summary>
/// Supported gameplay event types.
/// </summary>
public enum GameplayEventType
{
    /// <summary>Game SDK finished loading.</summary>
    GameLoadingFinished = 0,

    /// <summary>User started gameplay.</summary>
    GameplayStart = 1,

    /// <summary>User stopped gameplay.</summary>
    GameplayStop = 2,

    /// <summary>Commercial/ad break.</summary>
    CommercialBreak = 3,

    /// <summary>Rewarded ad break.</summary>
    RewardedBreak = 4,

    /// <summary>Game reported an error.</summary>
    CaptureError = 5,

    /// <summary>Performance measurement.</summary>
    Measure = 6
}
