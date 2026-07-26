using System;
using System.Threading.Tasks;
using GameHub.Multiplayer;
using Shouldly;
using Xunit;

namespace GameHub.Tests.Multiplayer
{
    public class MultiplayerPresenceStore_Tests : GameHubTestBase
    {
        private readonly IMultiplayerPresenceStore _store;

        public MultiplayerPresenceStore_Tests()
        {
            _store = LocalIocManager.Resolve<IMultiplayerPresenceStore>();
        }

        [Fact]
        public async Task Dado_EntradaRegistrada_Quando_ConsultarPorChave_Entao_RetornaPresenca()
        {
            var entry = CreateEntry();

            await _store.RegisterAsync(entry, TimeSpan.FromMinutes(1));

            var result = await _store.GetAsync(entry.TenantId, entry.MatchId, entry.ConnectionId);

            result.ShouldNotBeNull();
            result.UserId.ShouldBe(entry.UserId);
            result.InstanceId.ShouldBe(entry.InstanceId);
        }

        [Fact]
        public async Task Dado_EntradaRegistrada_Quando_Remover_Entao_NaoRetornaPresenca()
        {
            var entry = CreateEntry();
            await _store.RegisterAsync(entry, TimeSpan.FromMinutes(1));

            await _store.RemoveAsync(entry.TenantId, entry.MatchId, entry.ConnectionId);

            (await _store.GetAsync(entry.TenantId, entry.MatchId, entry.ConnectionId))
                .ShouldBeNull();
        }

        [Fact]
        public async Task Dado_EntradaRegistrada_Quando_Atualizar_Entao_AtualizaUltimaAtividade()
        {
            var entry = CreateEntry();
            await _store.RegisterAsync(entry, TimeSpan.FromMinutes(1));
            var originalLastSeen = entry.LastSeenAt;

            await Task.Delay(10);
            await _store.RefreshAsync(entry.TenantId, entry.MatchId, entry.ConnectionId, TimeSpan.FromMinutes(1));

            var refreshed = await _store.GetAsync(entry.TenantId, entry.MatchId, entry.ConnectionId);
            refreshed.ShouldNotBeNull();
            refreshed.LastSeenAt.ShouldBeGreaterThan(originalLastSeen);
        }

        [Fact]
        public async Task Dado_PresencaDeOutroTenant_Quando_Consultar_Entao_NaoRetornaEntrada()
        {
            var entry = CreateEntry();
            await _store.RegisterAsync(entry, TimeSpan.FromMinutes(1));

            var result = await _store.GetAsync(
                entry.TenantId + 1,
                entry.MatchId,
                entry.ConnectionId);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task Dado_TTLExpirado_Quando_Consultar_Entao_RetornaAusente()
        {
            var entry = CreateEntry();
            await _store.RegisterAsync(entry, TimeSpan.FromSeconds(1));

            await Task.Delay(TimeSpan.FromMilliseconds(1200));

            (await _store.GetAsync(entry.TenantId, entry.MatchId, entry.ConnectionId))
                .ShouldBeNull();
        }

        private static MultiplayerPresenceEntry CreateEntry()
        {
            return new MultiplayerPresenceEntry
            {
                TenantId = 1,
                GameId = Guid.NewGuid(),
                MatchId = Guid.NewGuid(),
                ConnectionId = Guid.NewGuid().ToString("N"),
                UserId = 42,
                InstanceId = "test-instance",
                JoinedAt = DateTimeOffset.UtcNow,
                LastSeenAt = DateTimeOffset.UtcNow
            };
        }
    }
}
