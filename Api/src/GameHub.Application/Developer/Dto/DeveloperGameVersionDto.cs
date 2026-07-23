using System;

namespace GameHub.Developer.Dto;

/// <summary>
/// Versão recente de um jogo no painel do desenvolvedor.
/// </summary>
public class DeveloperGameVersionDto
{
    /// <summary>Build identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Game identifier.</summary>
    public Guid GameId { get; set; }

    /// <summary>Game title.</summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>Version string.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Build status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Upload timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Publication timestamp, if published.</summary>
    public DateTime? PublishedAt { get; set; }
}
