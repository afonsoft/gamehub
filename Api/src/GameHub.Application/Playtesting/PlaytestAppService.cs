using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using GameHub.Playtesting.Dto;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Playtesting
{
    public class PlaytestAppService : GameHubAppServiceBase, IPlaytestAppService
    {
        private readonly IRepository<PlaytestSession, Guid> _playtestRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;

        public PlaytestAppService(
            IRepository<PlaytestSession, Guid> playtestRepository,
            IRepository<Game, Guid> gameRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository)
        {
            _playtestRepository = playtestRepository;
            _gameRepository = gameRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
        public async Task<PlaytestSessionDto> RequestPlaytestAsync(RequestPlaytestInput input)
        {
            await EnsureCurrentUserHasAccessToGameAsync(input.GameId);

            var playtest = new PlaytestSession
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                RequestedByUserId = AbpSession.UserId.Value,
                Status = PlaytestSessionStatus.Requested,
                Notes = input.Notes,
                CreatedAt = Clock.Now
            };

            await _playtestRepository.InsertAsync(playtest);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PlaytestSessionDto>(playtest);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
        public async Task<ListResultDto<PlaytestSessionDto>> GetPlaytestsByGameAsync(Guid gameId)
        {
            await EnsureCurrentUserHasAccessToGameAsync(gameId);

            var sessions = await _playtestRepository.GetAll()
                .Where(p => p.GameId == gameId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return new ListResultDto<PlaytestSessionDto>(ObjectMapper.Map<List<PlaytestSessionDto>>(sessions));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Review)]
        public async Task<PlaytestSessionDto> UploadRecordingAsync(UploadPlaytestRecordingInput input)
        {
            var playtest = await _playtestRepository.GetAsync(input.PlaytestId);
            playtest.RecordingUrl = input.RecordingUrl;
            playtest.Status = PlaytestSessionStatus.Completed;
            playtest.CompletedAt = Clock.Now;

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PlaytestSessionDto>(playtest);
        }

        private async Task EnsureCurrentUserHasAccessToGameAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAsync(gameId);

            var teamMember = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == AbpSession.UserId.Value);
            if (teamMember == null)
            {
                throw new AbpAuthorizationException("You must belong to a developer team to manage playtests.");
            }

            // Future: authorize by team ownership. For now, any team member can request a playtest for any game.
            // This keeps 24.2 decoupled from the game-team relationship until 24.1 is fully wired.
        }
    }
}
