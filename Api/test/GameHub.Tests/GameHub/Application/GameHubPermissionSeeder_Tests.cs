using System.Linq;
using GameHub.Authorization;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class GameHubPermissionSeeder_Tests : GameHubTestBase
    {
        [Fact]
        public void Dado_SeedInicial_Quando_ConsultarRoles_Entao_GameHubRolesExistentes()
        {
            UsingDbContext(context =>
            {
                var moderator = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.Name == "Moderator" && r.TenantId == 1);
                var developer = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.Name == "Developer" && r.TenantId == 1);
                var player = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.Name == "Player" && r.TenantId == 1);

                moderator.ShouldNotBeNull();
                developer.ShouldNotBeNull();
                player.ShouldNotBeNull();
                player.IsDefault.ShouldBeTrue();
            });
        }

        [Fact]
        public void Dado_SeedInicial_Quando_ConsultarPermissoes_Entao_GameHubPermissoesAtribuidas()
        {
            UsingDbContext(context =>
            {
                var permissions = context.RolePermissions.IgnoreQueryFilters().ToList();

                permissions.Any(p => p.Name == GameHubPermissions.Pages_Games_View).ShouldBeTrue();
                permissions.Any(p => p.Name == GameHubPermissions.Pages_Gameplay).ShouldBeTrue();
                permissions.Any(p => p.Name == GameHubPermissions.Pages_Leaderboard).ShouldBeTrue();
            });
        }
    }
}
