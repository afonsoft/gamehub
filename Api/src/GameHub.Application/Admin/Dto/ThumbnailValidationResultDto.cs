using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Result of validating a game thumbnail image.
    /// </summary>
    public class ThumbnailValidationResultDto
    {
        public Guid GameId { get; set; }

        public string Url { get; set; } = string.Empty;

        public bool IsValid { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public double? AspectRatio { get; set; }

        public long? SizeBytes { get; set; }

        public string Format { get; set; } = string.Empty;

        public bool IsWebP { get; set; }

        public string Error { get; set; } = string.Empty;
    }
}
