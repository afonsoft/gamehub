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
/// Result of a build upload and validation.
/// </summary>
public class UploadGameBuildResultDto
{
    /// <summary>Build unique identifier.</summary>
    public Guid BuildId { get; set; }

    /// <summary>Build version string.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Build status after validation.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Validation result summary.</summary>
    public ValidationSummaryDto ValidationSummary { get; set; }
}
