namespace GameHub.Gameplay.Dto;

/// <summary>
/// CSV export generated from the authorized game metrics query.
/// </summary>
public class GameMetricsExportDto
{
    /// <summary>Download file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Returned media type.</summary>
    public string ContentType { get; set; } = "text/csv";

    /// <summary>UTF-8 CSV content.</summary>
    public string Content { get; set; } = string.Empty;
}
