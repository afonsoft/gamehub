using System;

namespace GameHub.Admin.Dto
{
    /// <summary>
    /// Historical moderation review entry for a game.
    /// </summary>
    public class ModerationReviewHistoryItemDto
    {
        /// <summary>Review identifier.</summary>
        public Guid ReviewId { get; set; }

        /// <summary>Reviewer decision.</summary>
        public string Decision { get; set; } = string.Empty;

        /// <summary>Reviewer notes.</summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>Reviewer display name.</summary>
        public string ReviewerName { get; set; } = string.Empty;

        /// <summary>Creation timestamp.</summary>
        public DateTime CreatedAt { get; set; }
    }
}
