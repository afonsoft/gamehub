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
/// Input to publish a game to production.
/// </summary>
public class PublishGameInput
{
    /// <summary>Game to publish.</summary>
    [Required]
    public Guid GameId { get; set; }
}
