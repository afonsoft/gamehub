using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using GameHub;
using GameHub.Catalog;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public class GameplayAppService : GameHubAppServiceBase, IGameplayAppService
    {
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameplayEvent, Guid> _gameplayEventRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public GameplayAppService(
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository,
            IRepository<Game, Guid> gameRepository)
        {
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
            _gameRepository = gameRepository;
        }

        public async Task<PlaySessionDto> StartSessionAsync(StartPlaySessionInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);

            if (!string.IsNullOrEmpty(input.ClientRequestId))
            {
                var existingSession = await _playSessionRepository.FirstOrDefaultAsync(
                    s => s.GameId == input.GameId && s.ClientRequestId == input.ClientRequestId);

                if (existingSession != null)
                    return ObjectMapper.Map<PlaySessionDto>(existingSession);
            }

            var session = new PlaySession
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                StartedAt = DateTime.UtcNow,
                DeviceType = input.DeviceType,
                Browser = input.Browser ?? "Unknown",
                Referrer = input.Referrer,
                ClientRequestId = input.ClientRequestId
            };

            await _playSessionRepository.InsertAsync(session);
            game.TotalPlays++;
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<PlaySessionDto>(session);
        }

        public async Task<PlaySessionDto> StopSessionAsync(Guid sessionId)
        {
            var session = await _playSessionRepository.GetAsync(sessionId);
            session.EndedAt = DateTime.UtcNow;

            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<PlaySessionDto>(session);
        }

        public async Task EventAsync(GameplayEventInput input)
        {
            var session = await _playSessionRepository.GetAsync(input.SessionId);

            var ev = new GameplayEvent
            {
                Id = Guid.NewGuid(),
                PlaySessionId = input.SessionId,
                GameId = session.GameId,
                EventType = input.EventType,
                EventName = input.EventName,
                PayloadJson = input.PayloadJson,
                OccurredAt = DateTime.UtcNow
            };

            await _gameplayEventRepository.InsertAsync(ev);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
