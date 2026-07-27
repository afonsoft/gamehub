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
/// Input to complete a moderation review.
/// </summary>
public class CompleteReviewInput
{
    /// <summary>Review to complete.</summary>
    [Required]
    public Guid ReviewId { get; set; }

    /// <summary>Review decision.</summary>
    [Required]
    public ReviewDecision Decision { get; set; }

    /// <summary>Reviewer notes (required for rejections).</summary>
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>Identificador de idempotência enviado pelo cliente.</summary>
    [StringLength(64)]
    public string ClientRequestId { get; set; } = string.Empty;
}

/// <summary>
/// Possible moderation review decisions.
/// </summary>
public enum ReviewDecision
{
    /// <summary>Build is approved and can be published.</summary>
    Approved = 0,

    /// <summary>Build is rejected and cannot be published.</summary>
    Rejected = 1,

    /// <summary>Build requires changes before resubmission.</summary>
    RequiresChanges = 2
}
