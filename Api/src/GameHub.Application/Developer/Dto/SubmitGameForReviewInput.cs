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
/// Input to submit a game draft for moderation review.
/// </summary>
public class SubmitGameForReviewInput
{
    /// <summary>Game identifier.</summary>
    [Required]
    public Guid GameId { get; set; }

    /// <summary>Optional notes for the reviewer.</summary>
    [StringLength(1000)]
    public string Notes { get; set; }
}
