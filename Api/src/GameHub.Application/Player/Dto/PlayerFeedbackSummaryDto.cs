using System;
using System.Collections.Generic;

namespace GameHub.Player.Dto
{
    /// <summary>
    /// Summary of player feedback (ratings and reviews) for a game.
    /// </summary>
    public class PlayerFeedbackSummaryDto
    {
        public Guid GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public double AverageRating { get; set; }

        public long TotalReviews { get; set; }

        public Dictionary<int, long> Distribution { get; set; } = new();

        public double? SentimentScore { get; set; }

        public List<string> RecentComments { get; set; } = new();
    }
}
