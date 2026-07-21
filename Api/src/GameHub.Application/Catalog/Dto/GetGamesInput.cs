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
/// Input for paginated game catalog queries.
/// </summary>
public class GetGamesInput
{
    /// <summary>Number of items to skip.</summary>
    [Range(0, int.MaxValue)]
    public int SkipCount { get; set; }

    /// <summary>Maximum items per page.</summary>
    [Range(1, 100)]
    public int MaxResultCount { get; set; } = 24;

    /// <summary>Sort field: "Newest", "MostPlayed", "TopRated", "Title".</summary>
    public string Sorting { get; set; } = "Newest";

    /// <summary>Filter by category slug.</summary>
    public string CategorySlug { get; set; }

    /// <summary>Filter by tag slug.</summary>
    public string TagSlug { get; set; }

    /// <summary>Filter by device: "Desktop", "Mobile", "Tablet".</summary>
    public string Device { get; set; }

    /// <summary>Filter by orientation: "Portrait", "Landscape".</summary>
    public string Orientation { get; set; }
}
