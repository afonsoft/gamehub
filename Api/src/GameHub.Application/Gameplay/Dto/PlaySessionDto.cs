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
/// Active play session metadata.
/// </summary>
public class PlaySessionDto
{
    /// <summary>Session unique identifier.</summary>
    public Guid SessionId { get; set; }

    /// <summary>Game being played.</summary>
    public Guid GameId { get; set; }

    /// <summary>UTC timestamp when the session started.</summary>
    public DateTime StartedAt { get; set; }
}
