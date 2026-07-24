using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Gameplay.Dto;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameplayAppService_Tests : GameHubTestBase
    {
        private readonly IGameplayAppService _gameplayAppService;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;

        public GameplayAppService_Tests()
        {
            _gameplayAppService = LocalIocManager.Resolve<IGameplayAppService>();
            _gameRepository = LocalIocManager.Resolve<IRepository<Game, Guid>>();
            _playSessionRepository = LocalIocManager.Resolve<IRepository<PlaySession, Guid>>();
            _metricSnapshotRepository = LocalIocManager.Resolve<IRepository<GameMetricSnapshot, Guid>>();
        }

        [Fact]
        public async Task Dado_RequisicaoComClientRequestId_Quando_ChamarDuasVezes_Entao_RetornaMesmaSessaoENaoIncrementaTotalPlays()
        {
            var gameId = await SeedGameAsync();
            var clientRequestId = "client-req-123";
            var input = new StartPlaySessionInput
            {
                GameId = gameId,
                DeviceType = "Desktop",
                Browser = "Chrome",
                ClientRequestId = clientRequestId
            };

            var first = await _gameplayAppService.StartSessionAsync(input);
            var second = await _gameplayAppService.StartSessionAsync(input);

            first.SessionId.ShouldBe(second.SessionId);

            var sessions = await _playSessionRepository.GetAllListAsync(s => s.ClientRequestId == clientRequestId);
            sessions.Count.ShouldBe(1);

            var game = await _gameRepository.GetAsync(gameId);
            game.TotalPlays.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_RequisicoesSemClientRequestId_Quando_ChamarDuasVezes_Entao_CriaDuasSessoesEIncrementaTotalPlays()
        {
            var gameId = await SeedGameAsync();
            var input = new StartPlaySessionInput
            {
                GameId = gameId,
                DeviceType = "Desktop"
            };

            var first = await _gameplayAppService.StartSessionAsync(input);
            var second = await _gameplayAppService.StartSessionAsync(input);

            first.SessionId.ShouldNotBe(second.SessionId);

            var sessions = await _playSessionRepository.GetAllListAsync(s => s.GameId == gameId);
            sessions.Count.ShouldBe(2);

            var game = await _gameRepository.GetAsync(gameId);
            game.TotalPlays.ShouldBe(2);
        }

        [Fact]
        public async Task Dado_SessaoAtiva_Quando_AtualizarFps_Entao_DeveArmazenarValores()
        {
            var gameId = await SeedGameAsync();
            var input = new StartPlaySessionInput
            {
                GameId = gameId,
                DeviceType = "Desktop",
                Browser = "Chrome"
            };

            var session = await _gameplayAppService.StartSessionAsync(input);

            await _gameplayAppService.UpdateFpsAsync(new UpdateFpsInput
            {
                SessionId = session.SessionId,
                Average = 55.5,
                Min = 30.0
            });

            var updated = await _playSessionRepository.GetAsync(session.SessionId);
            updated.FpsAverage.ShouldBe(55.5);
            updated.FpsMin.ShouldBe(30.0);
        }

        [Fact]
        public async Task Dado_FpsDeMultiplasSessoes_Quando_Atualizar_Entao_AgregaMetricSnapshot()
        {
            var gameId = await SeedGameAsync();

            var first = await _gameplayAppService.StartSessionAsync(new StartPlaySessionInput { GameId = gameId, DeviceType = "Desktop", Browser = "Chrome" });
            await _gameplayAppService.UpdateFpsAsync(new UpdateFpsInput { SessionId = first.SessionId, Average = 60.0, Min = 40.0 });

            var second = await _gameplayAppService.StartSessionAsync(new StartPlaySessionInput { GameId = gameId, DeviceType = "Mobile", Browser = "Safari" });
            await _gameplayAppService.UpdateFpsAsync(new UpdateFpsInput { SessionId = second.SessionId, Average = 50.0, Min = 20.0 });

            var snapshot = await _metricSnapshotRepository.FirstOrDefaultAsync(m => m.GameId == gameId);
            snapshot.ShouldNotBeNull();
            snapshot.AvgFps.ShouldBe(55.0);
            snapshot.MinFps.ShouldBe(20.0);
        }

        private async Task<Guid> SeedGameAsync()
        {
            var profileId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            return await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "Tester",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, "Test Game", "test-game", "Test game", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Published,
                    TotalPlays = 0
                });

                await context.SaveChangesAsync();
                return gameId;
            });
        }
    }
}
