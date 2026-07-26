using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Moderation;
using GameHub.Player.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Player
{
    public class PlayerFeedbackAnalyticsAppService : GameHubAppServiceBase, IPlayerFeedbackAnalyticsAppService
    {
        private readonly IRepository<UserContent, Guid> _contentRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public PlayerFeedbackAnalyticsAppService(
            IRepository<UserContent, Guid> contentRepository,
            IRepository<Game, Guid> gameRepository)
        {
            _contentRepository = contentRepository;
            _gameRepository = gameRepository;
        }

        public async Task<PlayerFeedbackSummaryDto> GetFeedbackSummaryAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);

            var reviews = await _contentRepository.GetAll()
                .Where(c => c.GameId == gameId && c.ContentType == UserContentType.Review && c.IsApproved && !c.RequiresModeration)
                .Select(c => new { c.Rating, c.Text, c.CreationTime })
                .ToListAsync();

            var total = reviews.Count;
            var ratings = reviews.Where(r => r.Rating.HasValue).ToList();
            var average = ratings.Any() ? ratings.Average(r => r.Rating.Value) : 0.0;
            var distribution = ratings
                .GroupBy(r => r.Rating.Value)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            var comments = reviews
                .Where(r => !string.IsNullOrWhiteSpace(r.Text))
                .OrderByDescending(r => r.CreationTime)
                .Take(5)
                .Select(r => r.Text)
                .ToList();

            return new PlayerFeedbackSummaryDto
            {
                GameId = gameId,
                GameTitle = game.Title,
                AverageRating = average,
                TotalReviews = total,
                Distribution = distribution,
                SentimentScore = ComputeSentimentScore(comments),
                RecentComments = comments
            };
        }

        private static double? ComputeSentimentScore(List<string> comments)
        {
            if (comments.Count == 0)
            {
                return null;
            }

            var positive = new[] { "good", "great", "awesome", "love", "fun", "amazing", "nice", "excellent", "perfect", "bom", "ótimo", "legal", "divertido" };
            var negative = new[] { "bad", "terrible", "hate", "boring", "worst", "bug", "horrible", "slow", "ruim", "horrível", "chato", "lento" };

            var score = 0.0;
            foreach (var comment in comments)
            {
                var lower = comment.ToLowerInvariant();
                score += positive.Count(lower.Contains) * 0.5;
                score -= negative.Count(lower.Contains) * 0.5;
            }

            return Math.Clamp(score / comments.Count + 0.5, 0.0, 1.0);
        }
    }
}
