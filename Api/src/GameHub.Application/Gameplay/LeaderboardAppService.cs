using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Eaf.Middleware.Authorization.Users;
using GameHub.Gameplay.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Gameplay
{
    public class LeaderboardAppService : GameHubAppServiceBase, ILeaderboardAppService
    {
        private readonly ILeaderboardCache _leaderboardCache;
        private readonly IRepository<LeaderboardEntry, Guid> _leaderboardEntryRepository;
        private readonly IRepository<User, long> _userRepository;

        public LeaderboardAppService(
            ILeaderboardCache leaderboardCache,
            IRepository<LeaderboardEntry, Guid> leaderboardEntryRepository,
            IRepository<User, long> userRepository)
        {
            _leaderboardCache = leaderboardCache;
            _leaderboardEntryRepository = leaderboardEntryRepository;
            _userRepository = userRepository;
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
            var entries = (await _leaderboardCache.GetTopAsync(input.GameId, input.Take)).ToList();
            var userIds = entries.Select(e => e.UserId).Distinct().ToList();
            var users = await _userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.UserName })
                .ToDictionaryAsync(u => u.Id);

            foreach (var entry in entries)
            {
                var user = users.ContainsKey(entry.UserId) ? users[entry.UserId] : null;
                entry.DisplayName = !string.IsNullOrWhiteSpace(user?.FullName)
                    ? user.FullName
                    : (!string.IsNullOrWhiteSpace(user?.UserName) ? user.UserName : $"Player {entry.UserId}");
            }

            return new ListResultDto<LeaderboardEntryDto>(entries);
        }

        public async Task<LeaderboardEntryDto> GetMyRankAsync(GetLeaderboardInput input)
        {
            var userId = AbpSession.UserId ?? 0;
            var entry = await _leaderboardCache.GetMyRankAsync(input.GameId, userId);
            if (entry == null)
            {
                return null;
            }

            var user = await _userRepository.GetAll()
                .Where(u => u.Id == userId)
                .Select(u => new { u.FullName, u.UserName })
                .FirstOrDefaultAsync();

            entry.DisplayName = !string.IsNullOrWhiteSpace(user?.FullName)
                ? user.FullName
                : (!string.IsNullOrWhiteSpace(user?.UserName) ? user.UserName : $"Player {entry.UserId}");

            return entry;
        }
    }
}
