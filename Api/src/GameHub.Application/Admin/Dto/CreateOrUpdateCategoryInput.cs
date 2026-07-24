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
/// Input to create or update a category.
/// </summary>
public class CreateOrUpdateCategoryInput
{
    /// <summary>Category identifier (null for creation).</summary>
    public Guid? Id { get; set; }

    /// <summary>Category display name.</summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug (auto-generated if empty).</summary>
    [StringLength(100)]
    public string Slug { get; set; }

    /// <summary>Display sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Whether the category is active and visible.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>SEO description for the category page.</summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>SEO keywords for the category page.</summary>
    [StringLength(256)]
    public string Keywords { get; set; } = string.Empty;
}
