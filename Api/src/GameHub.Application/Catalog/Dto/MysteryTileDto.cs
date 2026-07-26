using System;

namespace GameHub.Catalog.Dto
{
    /// <summary>
    /// A mystery / discovery tile used for playtest sessions or featured games.
    /// </summary>
    public class MysteryTileDto
    {
        public Guid GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public bool IsPlaytest { get; set; }
        public string RecordingConsentPrompt { get; set; } = string.Empty;
    }
}
