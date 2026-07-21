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
/// Input to reject a game build after moderation review.
/// </summary>
public class RejectBuildInput
{
    /// <summary>Build to reject.</summary>
    [Required]
    public Guid GameBuildId { get; set; }

    /// <summary>Rejection reason (required).</summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
