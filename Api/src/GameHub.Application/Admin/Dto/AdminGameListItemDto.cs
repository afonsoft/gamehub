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
/// Game list item for the admin panel.
/// </summary>
public class AdminGameListItemDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Developer display name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>Game status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
