using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
namespace GameHub.Developer.Dto;

/// <summary>
/// Developer-facing game summary for the game list.
/// </summary>
public class GameSummaryDto
{
    /// <summary>Game unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Game status: "Draft", "InReview", "Published", "Suspended".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Currently published build version string.</summary>
    public string PublishedBuildVersion { get; set; }

    /// <summary>Total play count.</summary>
    public long TotalPlays { get; set; }

    /// <summary>UTC timestamp of the last metadata update.</summary>
    public DateTime LastUpdated { get; set; }
}
