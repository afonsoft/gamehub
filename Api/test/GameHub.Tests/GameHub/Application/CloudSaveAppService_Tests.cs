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
    public class CloudSaveAppService_Tests : GameHubTestBase
    {
        private readonly ICloudSaveAppService _cloudSaveAppService;
        private readonly IRepository<CloudSave, Guid> _cloudSaveRepository;

        public CloudSaveAppService_Tests()
        {
            _cloudSaveAppService = LocalIocManager.Resolve<ICloudSaveAppService>();
            _cloudSaveRepository = LocalIocManager.Resolve<IRepository<CloudSave, Guid>>();
        }

        [Fact]
        public async Task Dado_JogadorLogado_Quando_SalvarERecuperar_Entao_DadosPersistidos()
        {
            var gameId = await SeedGameAsync();
            const string data = "{\"level\":3,\"score\":1500}";

            await _cloudSaveAppService.SaveAsync(new SaveCloudSaveInput
            {
                GameId = gameId,
                Data = data
            });

            var saved = await _cloudSaveAppService.GetAsync(new GetCloudSaveInput
            {
                GameId = gameId
            });

            saved.Data.ShouldBe(data);

            var rows = await _cloudSaveRepository.GetAllListAsync(s => s.GameId == gameId);
            rows.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Dado_SaveMaiorQue1MB_Quando_Salvar_Entao_LancaExcecao()
        {
            var gameId = await SeedGameAsync();
            var huge = new string('x', 1_048_577);

            await Should.ThrowAsync<Abp.UI.UserFriendlyException>(async () =>
            {
                await _cloudSaveAppService.SaveAsync(new SaveCloudSaveInput
                {
                    GameId = gameId,
                    Data = huge
                });
            });
        }

        [Fact]
        public async Task Dado_JogadorLogado_Quando_Deletar_Entao_SaveRemovido()
        {
            var gameId = await SeedGameAsync();
            const string data = "{\"level\":3}";

            await _cloudSaveAppService.SaveAsync(new SaveCloudSaveInput
            {
                GameId = gameId,
                Data = data
            });

            await _cloudSaveAppService.DeleteAsync(new GetCloudSaveInput
            {
                GameId = gameId
            });

            var saved = await _cloudSaveAppService.GetAsync(new GetCloudSaveInput
            {
                GameId = gameId
            });

            saved.Data.ShouldBeNull();
            (await _cloudSaveRepository.GetAllListAsync(s => s.GameId == gameId)).Count.ShouldBe(0);
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
