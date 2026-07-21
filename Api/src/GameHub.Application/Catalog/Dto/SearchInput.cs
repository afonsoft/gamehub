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
/// Input for full-text game search with filters.
/// </summary>
public class SearchInput
{
    /// <summary>Search query string.</summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Query { get; set; } = string.Empty;

    /// <summary>Filter by category slugs.</summary>
    public List<string> Categories { get; set; }

    /// <summary>Filter by tag slugs.</summary>
    public List<string> Tags { get; set; }

    /// <summary>Filter by device.</summary>
    public string Device { get; set; }

    /// <summary>Filter by orientation.</summary>
    public string Orientation { get; set; }

    /// <summary>Number of items to skip.</summary>
    [Range(0, int.MaxValue)]
    public int SkipCount { get; set; }

    /// <summary>Maximum items per page.</summary>
    [Range(1, 100)]
    public int MaxResultCount { get; set; } = 24;

    /// <summary>Sort field.</summary>
    public string Sorting { get; set; } = "Relevance";
}
