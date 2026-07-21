using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public class GameplayAppService : ApplicationService, IGameplayAppService
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
            await _gameRepository.GetAsync(input.GameId);

            var session = new PlaySession
            {
                Id = Guid.NewGuid(),
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                StartedAt = DateTime.UtcNow,
                DeviceType = input.DeviceType,
                Browser = input.Browser,
                Referrer = input.Referrer
            };

            await _playSessionRepository.InsertAsync(session);
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

        public async Task EventAsync(Guid sessionId, GameplayEventInput input)
        {
            var session = await _playSessionRepository.GetAsync(sessionId);

            var ev = new GameplayEvent
            {
                Id = Guid.NewGuid(),
                PlaySessionId = sessionId,
                GameId = session.GameId,
                EventType = (GameHub.GameplayEventType)(int)input.EventType,
                EventName = input.EventName,
                PayloadJson = input.PayloadJson,
                OccurredAt = DateTime.UtcNow
            };

            await _gameplayEventRepository.InsertAsync(ev);
        }
    }
}
