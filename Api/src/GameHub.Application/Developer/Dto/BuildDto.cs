using GameHub.Authentication.Dto;
using GameHub.Admin.Dto;
using GameHub.Gameplay.Dto;
using GameHub.Developer.Dto;
using GameHub.Catalog.Dto;
using System.Linq;
using System.Collections.Generic;
using System;
namespace GameHub.Developer.Dto;

/// <summary>
/// Build metadata.
/// </summary>
public class BuildDto
{
    /// <summary>Build unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Version string (e.g., "1.0.0").</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Monotonically increasing build number.</summary>
    public int BuildNumber { get; set; }

    /// <summary>Build status: "Uploading", "Validating", "Valid", "Invalid", "Published".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Build zip size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>SHA-256 hash of the build zip.</summary>
    public string HashSha256 { get; set; } = string.Empty;

    /// <summary>Validation result summary (JSON or structured).</summary>
    public string ValidationSummary { get; set; }

    /// <summary>UTC timestamp when the build was uploaded.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the build was published (null if not published).</summary>
    public DateTime? PublishedAt { get; set; }
}
