using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Catalog.Dto;

/// <summary>
/// Compact game representation for cards and lists.
/// </summary>
public class GameCardDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug derived from title.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Short description for card display (max 160 chars).</summary>
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Assigned categories.</summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>Whether the game supports mobile devices.</summary>
    public bool SupportsMobile { get; set; }

    /// <summary>Whether the game supports desktop browsers.</summary>
    public bool SupportsDesktop { get; set; }

    /// <summary>Total play count across all sessions.</summary>
    public long TotalPlays { get; set; }
}
