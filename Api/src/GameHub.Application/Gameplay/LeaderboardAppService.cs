using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public class LeaderboardAppService : ApplicationService, ILeaderboardAppService
    {
        private readonly ILeaderboardCache _leaderboardCache;
        private readonly IRepository<LeaderboardEntry, Guid> _leaderboardEntryRepository;

        public LeaderboardAppService(
            ILeaderboardCache leaderboardCache,
            IRepository<LeaderboardEntry, Guid> leaderboardEntryRepository)
        {
            _leaderboardCache = leaderboardCache;
            _leaderboardEntryRepository = leaderboardEntryRepository;
        }

        public async Task SubmitScoreAsync(SubmitScoreInput input)
        {
            var userId = AbpSession.UserId ?? 0;

            await _leaderboardCache.SubmitScoreAsync(input.GameId, userId, input.Score);

            var existing = await _leaderboardEntryRepository.FirstOrDefaultAsync(
                e => e.GameId == input.GameId && e.UserId == userId);

            if (existing == null)
            {
                await _leaderboardEntryRepository.InsertAsync(new LeaderboardEntry
                {
                    Id = Guid.NewGuid(),
                    GameId = input.GameId,
                    UserId = userId,
                    Score = input.Score,
                    MetadataJson = input.MetadataJson,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (input.Score > existing.Score)
            {
                existing.Score = input.Score;
                existing.MetadataJson = input.MetadataJson;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<ListResultDto<LeaderboardEntryDto>> GetTopAsync(GetLeaderboardInput input)
        {
            var entries = await _leaderboardCache.GetTopAsync(input.GameId, input.Take);
            return new ListResultDto<LeaderboardEntryDto>(entries);
        }
    }
}
