using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Developer.Dto;

/// <summary>
/// Input to update game metadata.
/// </summary>
public class UpdateGameMetadataInput
{
    /// <summary>Game to update.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Short description for listings.</summary>
    [Required]
    [StringLength(200, MinimumLength = 10)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full description.</summary>
    [StringLength(50000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions.</summary>
    [StringLength(10000)]
    public string Instructions { get; set; }

    /// <summary>Control scheme for desktop and mobile.</summary>
    [StringLength(4000)]
    public string Controls { get; set; }

    /// <summary>Developer-suggested detailed description for moderator review.</summary>
    [StringLength(4000)]
    public string SuggestedDescription { get; set; }

    /// <summary>SEO-friendly description used in public meta tags.</summary>
    [StringLength(500)]
    public string SeoDescription { get; set; }

    /// <summary>URL of the privacy policy. Required if the build contains external requests.</summary>
    [StringLength(512)]
    public string PrivacyPolicyUrl { get; set; }

    /// <summary>Age rating.</summary>
    [Required]
    [StringLength(10)]
    public string AgeRating { get; set; } = "E";

    /// <summary>Orientation.</summary>
    [Required]
    [StringLength(20)]
    public string Orientation { get; set; } = "Both";

    /// <summary>Supports desktop browsers.</summary>
    public bool SupportsDesktop { get; set; } = true;

    /// <summary>Supports mobile devices.</summary>
    public bool SupportsMobile { get; set; }

    /// <summary>Supports tablet devices.</summary>
    public bool SupportsTablet { get; set; }

    /// <summary>Category identifiers to assign.</summary>
    public List<Guid> CategoryIds { get; set; }

    /// <summary>Tag identifiers to assign.</summary>
    public List<Guid> TagIds { get; set; }
}
