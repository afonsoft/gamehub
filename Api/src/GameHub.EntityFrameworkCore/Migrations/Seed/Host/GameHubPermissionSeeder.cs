using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Authorization.Roles;
using Eaf.Middleware.Authorization.Roles;
using GameHub.Authorization;
using GameHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Migrations.Seed.Host
{
    /// <summary>
    /// Seeds GameHub roles and their default permissions.
    /// </summary>
    public class GameHubPermissionSeeder
    {
        private readonly GameHubDbContext _context;

        public GameHubPermissionSeeder(GameHubDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            _context.SuppressAutoSetTenantId = true;

            SeedHostRoles();
            SeedDefaultTenantRoles();

            _context.SaveChanges();
        }

        private void SeedHostRoles()
        {
            var admin = GetOrCreateRole(
                tenantId: null,
                StaticRoleNames.Host.Admin,
                StaticRoleNames.Host.Admin,
                isStatic: true,
                isDefault: false);

            GrantPermissions(admin, AllPermissions());

            var developer = GetOrCreateRole(
                tenantId: null,
                "Developer",
                "Developer",
                isStatic: false,
                isDefault: false);

            GrantPermissions(developer, DeveloperPermissions());

            var player = GetOrCreateRole(
                tenantId: null,
                "Player",
                "Player",
                isStatic: false,
                isDefault: true);

            GrantPermissions(player, PlayerPermissions());
        }

        private void SeedDefaultTenantRoles()
        {
            const int defaultTenantId = 1;
            const int playerTenantId = 2;

            var admin = GetOrCreateRole(
                defaultTenantId,
                StaticRoleNames.Tenants.Admin,
                StaticRoleNames.Tenants.Admin,
                isStatic: true,
                isDefault: false);
            GrantPermissions(admin, AdminPermissions());

            var moderator = GetOrCreateRole(
                defaultTenantId,
                "Moderator",
                "Moderator",
                isStatic: false,
                isDefault: false);
            GrantPermissions(moderator, ModeratorPermissions());

            var developer = GetOrCreateRole(
                defaultTenantId,
                "Developer",
                "Developer",
                isStatic: false,
                isDefault: false);
            GrantPermissions(developer, DeveloperPermissions());

            var player = GetOrCreateRole(
                defaultTenantId,
                "Player",
                "Player",
                isStatic: false,
                isDefault: true);
            GrantPermissions(player, PlayerPermissions());

            // Player tenant roles
            var playerTenantPlayer = GetOrCreateRole(
                playerTenantId,
                "Player",
                "Player",
                isStatic: false,
                isDefault: true);
            GrantPermissions(playerTenantPlayer, PlayerPermissions());
        }

        private Role GetOrCreateRole(int? tenantId, string name, string displayName, bool isStatic, bool isDefault)
        {
            var role = _context.Roles
                .IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == tenantId && r.Name == name);

            if (role != null)
            {
                return role;
            }

            role = new Role(tenantId, name, displayName)
            {
                IsStatic = isStatic,
                IsDefault = isDefault
            };

            _context.Roles.Add(role);
            _context.SaveChanges();

            return role;
        }

        private void GrantPermissions(Role role, IEnumerable<string> permissions)
        {
            var granted = _context.RolePermissions
                .IgnoreQueryFilters()
                .Where(p => p.TenantId == role.TenantId && p.RoleId == role.Id)
                .Select(p => p.Name)
                .ToHashSet();

            foreach (var permission in permissions.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
            {
                if (granted.Contains(permission))
                {
                    continue;
                }

                _context.RolePermissions.Add(new RolePermissionSetting
                {
                    TenantId = role.TenantId,
                    RoleId = role.Id,
                    Name = permission,
                    IsGranted = true
                });
            }

            _context.SaveChanges();
        }

        private static IEnumerable<string> AllPermissions()
        {
            return GameHubPermissions.AllPermissions();
        }

        private static IEnumerable<string> AdminPermissions()
        {
            return GameHubPermissions.AdminPermissions();
        }

        private static IEnumerable<string> ModeratorPermissions()
        {
            return GameHubPermissions.ModeratorPermissions();
        }

        private static IEnumerable<string> DeveloperPermissions()
        {
            return GameHubPermissions.DeveloperPermissions();
        }

        private static IEnumerable<string> PlayerPermissions()
        {
            return GameHubPermissions.PlayerPermissions();
        }
    }
}
