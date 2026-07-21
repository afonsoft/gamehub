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
/// Input to create or update a tag.
/// </summary>
public class CreateOrUpdateTagInput
{
    /// <summary>Tag identifier (null for creation).</summary>
    public Guid? Id { get; set; }

    /// <summary>Tag display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug (auto-generated if empty).</summary>
    [StringLength(100)]
    public string Slug { get; set; }
}
