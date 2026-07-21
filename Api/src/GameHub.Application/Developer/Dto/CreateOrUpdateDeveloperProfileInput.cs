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
/// Input to create or update a developer profile.
/// </summary>
public class CreateOrUpdateDeveloperProfileInput
{
    /// <summary>Public display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Legal company name.</summary>
    [StringLength(200)]
    public string LegalName { get; set; }

    /// <summary>Developer website URL.</summary>
    [Url]
    [StringLength(500)]
    public string WebsiteUrl { get; set; }

    /// <summary>Support email for players.</summary>
    [EmailAddress]
    [StringLength(256)]
    public string SupportEmail { get; set; }
}
