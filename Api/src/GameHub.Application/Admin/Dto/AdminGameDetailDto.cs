using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
namespace GameHub.Admin.Dto;

/// <summary>
/// Full game detail for the admin panel, including build and moderation history.
/// </summary>
public class AdminGameDetailDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Short description.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Game instructions.</summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Control scheme for desktop and mobile.</summary>
    public string Controls { get; set; } = string.Empty;

    /// <summary>Age rating.</summary>
    public string AgeRating { get; set; } = string.Empty;

    /// <summary>Orientation.</summary>
    public string Orientation { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Hero/banner image URL.</summary>
    public string HeroImageUrl { get; set; } = string.Empty;

    /// <summary>Developer display name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>Game status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Average rating.</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Total votes used to compute the average rating.</summary>
    public long TotalVotes { get; set; }

    /// <summary>Full build history.</summary>
    public List<BuildDto> BuildHistory { get; set; } = new();

    /// <summary>Full moderation review history.</summary>
    public List<ModerationReviewDto> ModerationHistory { get; set; } = new();

    /// <summary>Assigned categories.</summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>Assigned tags.</summary>
    public List<TagDto> Tags { get; set; } = new();

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
