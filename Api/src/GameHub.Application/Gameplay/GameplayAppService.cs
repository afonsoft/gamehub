using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub;
using GameHub.Catalog;
using GameHub.Gameplay.Dto;
using GameHub.Player;
using GameHub.Player.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Gameplay
{
    public class GameplayAppService : GameHubAppServiceBase, IGameplayAppService
    {
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameplayEvent, Guid> _gameplayEventRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IPlayerAccountAppService _playerAccountAppService;

        public GameplayAppService(
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameplayEvent, Guid> gameplayEventRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<Game, Guid> gameRepository,
            IPlayerAccountAppService playerAccountAppService)
        {
            _playSessionRepository = playSessionRepository;
            _gameplayEventRepository = gameplayEventRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _gameRepository = gameRepository;
            _playerAccountAppService = playerAccountAppService;
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
                TrafficSource = input.TrafficSource,
                UtmSource = input.UtmSource,
                UtmMedium = input.UtmMedium,
                UtmCampaign = input.UtmCampaign,
                ClientRequestId = input.ClientRequestId
            };

            await _playSessionRepository.InsertAsync(session);
            game.TotalPlays++;
            await CurrentUnitOfWork.SaveChangesAsync();

            await _playerAccountAppService.TrackPlayAsync(new TrackPlayInput { GameId = input.GameId });

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

        public async Task UpdateFpsAsync(UpdateFpsInput input)
        {
            var session = await _playSessionRepository.GetAsync(input.SessionId);

            session.FpsAverage = input.Average;
            session.FpsMin = input.Min;

            await CurrentUnitOfWork.SaveChangesAsync();
            await UpdateMetricSnapshotFpsAsync(session.GameId);
        }

        private async Task UpdateMetricSnapshotFpsAsync(Guid gameId)
        {
            var today = Clock.Now.Date;
            var sessions = (await _playSessionRepository.GetAll()
                .Where(s => s.GameId == gameId && s.StartedAt.Year == today.Year && s.StartedAt.Month == today.Month && s.StartedAt.Day == today.Day && s.FpsAverage.HasValue && s.FpsMin.HasValue)
                .ToListAsync())
                .ToList();

            if (!sessions.Any())
            {
                return;
            }

            var avgFps = sessions.Average(s => s.FpsAverage.Value);
            var minFps = sessions.Min(s => s.FpsMin.Value);

            var metrics = await _metricSnapshotRepository.GetAll()
                .Where(m => m.GameId == gameId)
                .ToListAsync();
            var metric = metrics.FirstOrDefault(m => m.Date.Year == today.Year && m.Date.Month == today.Month && m.Date.Day == today.Day);

            if (metric == null)
            {
                metric = new GameMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    Date = today
                };
                await _metricSnapshotRepository.InsertAsync(metric);
            }

            metric.AvgFps = avgFps;
            metric.MinFps = minFps;
        }
    }
}
