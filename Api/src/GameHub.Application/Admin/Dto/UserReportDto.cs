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
/// User-submitted report about a game.
/// </summary>
public class UserReportDto
{
    /// <summary>Report unique identifier.</summary>
    public Guid ReportId { get; set; }

    /// <summary>Game being reported.</summary>
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>User who submitted the report.</summary>
    public long UserId { get; set; }

    /// <summary>Report reason category.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Free-text description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Report status: "Open", "UnderReview", "Resolved", "Dismissed".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the report was created.</summary>
    public DateTime CreatedAt { get; set; }
}
