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
/// Input to approve a game build after moderation review.
/// </summary>
public class ApproveBuildInput
{
    /// <summary>Build to approve.</summary>
    [Required]
    public Guid GameBuildId { get; set; }

    /// <summary>Optional approval notes.</summary>
    [StringLength(1000)]
    public string Notes { get; set; }
}
