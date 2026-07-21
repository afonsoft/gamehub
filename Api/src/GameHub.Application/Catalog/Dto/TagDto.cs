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
/// Tag lookup entry.
/// </summary>
public class TagDto
{
    /// <summary>Tag unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Tag display name.</summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;
}
