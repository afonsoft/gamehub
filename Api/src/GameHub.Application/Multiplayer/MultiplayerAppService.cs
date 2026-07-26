using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Multiplayer.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Multiplayer
{
    /// <summary>
    /// Application service for multiplayer match management.
    /// </summary>
    [AbpAllowAnonymous]
    public class MultiplayerAppService : GameHubAppServiceBase, IMultiplayerAppService
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IRepository<MatchState, Guid> _matchRepository;

        public MultiplayerAppService(
            IMatchmakingService matchmakingService,
            IRepository<MatchState, Guid> matchRepository)
        {
            _matchmakingService = matchmakingService;
            _matchRepository = matchRepository;
        }

        public async Task<MatchDto> CreateMatchAsync(CreateMatchInput input)
        {
            var match = await _matchmakingService.CreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            return await GetMatchAsync(match.Id);
        }

        public async Task<MatchDto> CreateOrJoinMatchAsync(CreateMatchInput input)
        {
            var match = await _matchmakingService.FindOrCreateMatchAsync(input.GameId, input.Mode, input.MaxPlayers);
            return await GetMatchAsync(match.Id);
        }

        public async Task<MatchDto> JoinMatchAsync(JoinMatchInput input)
        {
            var userId = AbpSession.UserId;
            await _matchmakingService.JoinMatchAsync(input.MatchId, userId, input.AnonymousIdHash, input.ConnectionId);
            return await GetMatchAsync(input.MatchId);
        }

        public async Task<MatchDto> JoinMatchByRoomCodeAsync(JoinMatchByRoomCodeInput input)
        {
            var match = await _matchRepository.GetAll()
                .Include(m => m.Participants)
                .Where(m => m.RoomCode == input.RoomCode && m.Status != MatchStatus.Ended && m.ExpiresAt > Clock.Now)
                .OrderByDescending(m => m.CreationTime)
                .FirstOrDefaultAsync();

            if (match == null)
            {
                throw new InvalidOperationException("Match not found or expired.");
            }

            var userId = AbpSession.UserId;
            await _matchmakingService.JoinMatchAsync(match.Id, userId, input.AnonymousIdHash, input.ConnectionId);
            return await GetMatchAsync(match.Id);
        }

        public async Task LeaveMatchAsync(LeaveMatchInput input)
        {
            await _matchmakingService.LeaveMatchAsync(input.MatchId, input.ConnectionId);
        }

        public async Task<MatchDto> GetMatchAsync(Guid matchId)
        {
            var match = await _matchRepository.GetAll()
                .Include(m => m.Participants)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                throw new InvalidOperationException("Match not found.");
            }

            return ObjectMapper.Map<MatchDto>(match);
        }

        public async Task UpdateMatchStateAsync(UpdateMatchStateInput input)
        {
            await _matchmakingService.UpdateMatchStateAsync(input.MatchId, input.PayloadJson);
        }

        public async Task EndMatchAsync(Guid matchId)
        {
            await _matchmakingService.EndMatchAsync(matchId);
        }
    }
}
