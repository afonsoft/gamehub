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
/// Developer profile data.
/// </summary>
public class DeveloperProfileDto
{
    /// <summary>Profile unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Public display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Legal company name (for invoices).</summary>
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

    /// <summary>Profile status: "Pending", "Active", "Suspended".</summary>
    public string Status { get; set; } = "Pending";
}
