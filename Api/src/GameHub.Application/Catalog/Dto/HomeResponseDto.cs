using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
namespace GameHub.Catalog.Dto;

/// <summary>
/// Home page aggregated content.
/// </summary>
public class HomeResponseDto
{
    /// <summary>Featured/highlighted games curated by admin.</summary>
    public List<GameCardDto> Highlights { get; set; } = new();

    /// <summary>Recently published games.</summary>
    public List<GameCardDto> NewGames { get; set; } = new();

    /// <summary>Games with the most plays in the current period.</summary>
    public List<GameCardDto> MostPlayed { get; set; } = new();

    /// <summary>Games trending by recent play growth.</summary>
    public List<GameCardDto> Trending { get; set; } = new();

    /// <summary>All active categories for the sidebar/chips.</summary>
    public List<CategoryDto> Categories { get; set; } = new();
}
