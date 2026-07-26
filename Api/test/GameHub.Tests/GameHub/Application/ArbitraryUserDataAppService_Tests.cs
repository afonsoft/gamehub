using System;
using System.Threading.Tasks;
using GameHub.ArbitraryUserData;
using GameHub.ArbitraryUserData.Dto;
using GameHub.Catalog;
using GameHub.Developers;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class ArbitraryUserDataAppService_Tests : GameHubTestBase
    {
        private readonly IArbitraryUserDataAppService _arbitraryUserDataAppService;

        public ArbitraryUserDataAppService_Tests()
        {
            _arbitraryUserDataAppService = LocalIocManager.Resolve<IArbitraryUserDataAppService>();
        }

        [Fact]
        public async Task Dado_DadosArbitrarios_Quando_SetEGet_Entao_RetornaValor()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("AUDS Game", "auds-game");

            await _arbitraryUserDataAppService.SetAsync(new SetArbitraryUserDataInput
            {
                GameId = gameId,
                Key = "progress",
                ValueJson = "{\"level\":5}",
                TtlSeconds = 3600
            });

            var value = await _arbitraryUserDataAppService.GetAsync(new GetArbitraryUserDataInput
            {
                GameId = gameId,
                Key = "progress"
            });

            value.ShouldBe("{\"level\":5}");
        }

        [Fact]
        public async Task Dado_ChaveReservada_Quando_Set_Entao_LancaExcecao()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("AUDS Game", "auds-game-reserved");

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _arbitraryUserDataAppService.SetAsync(new SetArbitraryUserDataInput
                {
                    GameId = gameId,
                    Key = "gamehub_ignore_test",
                    ValueJson = "{}"
                });
            });
        }

        [Fact]
        public async Task Dado_JsonInvalido_Quando_Set_Entao_LancaExcecao()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("AUDS Game", "auds-game-json");

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _arbitraryUserDataAppService.SetAsync(new SetArbitraryUserDataInput
                {
                    GameId = gameId,
                    Key = "bad",
                    ValueJson = "not json"
                });
            });
        }

        [Fact]
        public async Task Dado_DadosDeletados_Quando_Get_Entao_RetornaVazio()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("AUDS Game", "auds-game-delete");

            await _arbitraryUserDataAppService.SetAsync(new SetArbitraryUserDataInput
            {
                GameId = gameId,
                Key = "temp",
                ValueJson = "{\"x\":1}"
            });

            await _arbitraryUserDataAppService.DeleteAsync(new DeleteArbitraryUserDataInput
            {
                GameId = gameId,
                Key = "temp"
            });

            var value = await _arbitraryUserDataAppService.GetAsync(new GetArbitraryUserDataInput
            {
                GameId = gameId,
                Key = "temp"
            });

            value.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Dado_DadosArmazenados_Quando_Quota_Entao_RetornaTotais()
        {
            LoginAsDefaultTenantAdmin();
            var gameId = await SeedGameAsync("AUDS Game", "auds-game-quota");

            await _arbitraryUserDataAppService.SetAsync(new SetArbitraryUserDataInput
            {
                GameId = gameId,
                Key = "a",
                ValueJson = "{\"x\":1}"
            });

            var quota = await _arbitraryUserDataAppService.GetQuotaAsync(gameId);

            quota.TotalKeys.ShouldBe(1);
            quota.TotalBytes.ShouldBeGreaterThan(0);
            quota.MaxKeys.ShouldBe(ArbitraryUserDataAppService.MaxKeys);
            quota.MaxBytesPerValue.ShouldBe(ArbitraryUserDataAppService.MaxBytesPerValue);
        }

        private async Task<Guid> SeedGameAsync(string title, string slug)
        {
            var gameId = Guid.NewGuid();
            var profileId = Guid.NewGuid();
            await UsingDbContextAsync(async context =>
            {
                await context.DeveloperProfiles.AddAsync(new DeveloperProfile
                {
                    Id = profileId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId ?? 1,
                    DisplayName = "Tester",
                    Status = DeveloperProfileStatus.Active
                });

                await context.Games.AddAsync(new Game(gameId, title, slug, "Test", profileId)
                {
                    TenantId = AbpSession.TenantId,
                    Status = GameStatus.Draft
                });
            });

            return gameId;
        }
    }
}
