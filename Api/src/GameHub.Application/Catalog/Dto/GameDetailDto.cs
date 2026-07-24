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
/// Full game representation for the detail page.
/// </summary>
public class GameDetailDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Short description for listings.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full HTML/Markdown description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions (HTML/Markdown).</summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Control scheme for desktop and mobile (HTML/Markdown).</summary>
    public string Controls { get; set; } = string.Empty;

    /// <summary>Developer-suggested detailed description for moderator review.</summary>
    public string SuggestedDescription { get; set; } = string.Empty;

    /// <summary>SEO-friendly description used in public meta tags.</summary>
    public string SeoDescription { get; set; } = string.Empty;

    /// <summary>Age rating (e.g., E, E10+, T, M).</summary>
    public string AgeRating { get; set; } = string.Empty;

    /// <summary>Game orientation: Portrait, Landscape, or Both.</summary>
    public string Orientation { get; set; } = string.Empty;

    /// <summary>Hero/banner image URL for detail page.</summary>
    public string HeroImageUrl { get; set; } = string.Empty;

    /// <summary>Developer display name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>URL to the published build's index.html.</summary>
    public string PublishedBuildUrl { get; set; } = string.Empty;

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Total like votes.</summary>
    public long TotalLikes { get; set; }

    /// <summary>Total dislike votes.</summary>
    public long TotalDislikes { get; set; }

    /// <summary>Average user rating (0–5).</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Total votes used to compute the average rating.</summary>
    public long TotalVotes { get; set; }

    /// <summary>Supports desktop browsers.</summary>
    public bool SupportsDesktop { get; set; }

    /// <summary>Supports mobile browsers.</summary>
    public bool SupportsMobile { get; set; }

    /// <summary>Supports tablet browsers.</summary>
    public bool SupportsTablet { get; set; }

    /// <summary>Assigned categories.</summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>Assigned tags.</summary>
    public List<TagDto> Tags { get; set; } = new();

    /// <summary>Related/similar games.</summary>
    public List<GameCardDto> RelatedGames { get; set; } = new();
}
