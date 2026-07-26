namespace GameHub.Developer.Dto
{
    /// <summary>
    /// Warning about an image asset that could be optimized inside a build package.
    /// </summary>
    public class ImageOptimizationWarningDto
    {
        /// <summary>Path of the image file inside the package.</summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>Current size in bytes.</summary>
        public long CurrentSizeBytes { get; set; }

        /// <summary>Estimated bytes that could be saved by compressing or converting to WebP.</summary>
        public long EstimatedSavingsBytes { get; set; }

        /// <summary>Human-readable recommendation.</summary>
        public string Recommendation { get; set; } = string.Empty;
    }
}
