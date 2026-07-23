using System;

namespace GameHub.Developer.Dto;

/// <summary>
/// Ação pendente no painel do desenvolvedor.
/// </summary>
public class DeveloperDashboardActionDto
{
    /// <summary>Game identifier.</summary>
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Game slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Current status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Suggested action.</summary>
    public string Action { get; set; } = string.Empty;
}
