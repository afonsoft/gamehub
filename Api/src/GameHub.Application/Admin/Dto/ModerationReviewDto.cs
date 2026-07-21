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
/// Moderation review record.
/// </summary>
public class ModerationReviewDto
{
    /// <summary>Review unique identifier.</summary>
    public Guid ReviewId { get; set; }

    /// <summary>Game being reviewed.</summary>
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>Build being reviewed.</summary>
    public Guid GameBuildId { get; set; }

    /// <summary>Reviewer display name.</summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>Review status: "Pending", "InProgress", "Completed".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Review decision: "Approved", "Rejected", "RequiresChanges".</summary>
    public string Decision { get; set; }

    /// <summary>Reviewer notes.</summary>
    public string Notes { get; set; }

    /// <summary>UTC timestamp when the review was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the review was completed (null if pending).</summary>
    public DateTime? CompletedAt { get; set; }
}
