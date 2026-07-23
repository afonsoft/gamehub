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
            return new[]
            {
                GameHubPermissions.Pages_Games,
                GameHubPermissions.Pages_Games_View,
                GameHubPermissions.Pages_Games_Create,
                GameHubPermissions.Pages_Games_Edit,
                GameHubPermissions.Pages_Games_Delete,
                GameHubPermissions.Pages_Games_Publish,
                GameHubPermissions.Pages_Games_Suspend,
                GameHubPermissions.Pages_Builds,
                GameHubPermissions.Pages_Builds_Upload,
                GameHubPermissions.Pages_Builds_View,
                GameHubPermissions.Pages_Builds_Approve,
                GameHubPermissions.Pages_Builds_Reject,
                GameHubPermissions.Pages_Moderation,
                GameHubPermissions.Pages_Moderation_View,
                GameHubPermissions.Pages_Moderation_Review,
                GameHubPermissions.Pages_Moderation_Complete,
                GameHubPermissions.Pages_Categories,
                GameHubPermissions.Pages_Categories_Manage,
                GameHubPermissions.Pages_Tags,
                GameHubPermissions.Pages_Tags_Manage,
                GameHubPermissions.Pages_Dashboard,
                GameHubPermissions.Pages_Dashboard_View,
                GameHubPermissions.Pages_Dashboard_FeatureFlags,
                GameHubPermissions.Pages_Dashboard_AuditLog,
                GameHubPermissions.Pages_Users,
                GameHubPermissions.Pages_Users_Manage,
                GameHubPermissions.Pages_Reports,
                GameHubPermissions.Pages_Reports_View,
                GameHubPermissions.Pages_Reports_Manage,
                GameHubPermissions.Pages_Developer,
                GameHubPermissions.Pages_Developer_Profile,
                GameHubPermissions.Pages_Developer_Games,
                GameHubPermissions.Pages_Gameplay,
                GameHubPermissions.Pages_Leaderboard
            };
        }

        private static IEnumerable<string> AdminPermissions()
        {
            return new[]
            {
                GameHubPermissions.Pages_Games,
                GameHubPermissions.Pages_Games_View,
                GameHubPermissions.Pages_Games_Publish,
                GameHubPermissions.Pages_Games_Suspend,
                GameHubPermissions.Pages_Builds,
                GameHubPermissions.Pages_Builds_View,
                GameHubPermissions.Pages_Builds_Approve,
                GameHubPermissions.Pages_Builds_Reject,
                GameHubPermissions.Pages_Moderation,
                GameHubPermissions.Pages_Moderation_View,
                GameHubPermissions.Pages_Categories,
                GameHubPermissions.Pages_Categories_Manage,
                GameHubPermissions.Pages_Tags,
                GameHubPermissions.Pages_Tags_Manage,
                GameHubPermissions.Pages_Dashboard,
                GameHubPermissions.Pages_Dashboard_View,
                GameHubPermissions.Pages_Dashboard_FeatureFlags,
                GameHubPermissions.Pages_Dashboard_AuditLog,
                GameHubPermissions.Pages_Developer,
                GameHubPermissions.Pages_Developer_Profile,
                GameHubPermissions.Pages_Developer_Games,
                GameHubPermissions.Pages_Users,
                GameHubPermissions.Pages_Users_Manage,
                GameHubPermissions.Pages_Gameplay,
                GameHubPermissions.Pages_Leaderboard
            };
        }

        private static IEnumerable<string> ModeratorPermissions()
        {
            return new[]
            {
                GameHubPermissions.Pages_Games_View,
                GameHubPermissions.Pages_Builds,
                GameHubPermissions.Pages_Builds_View,
                GameHubPermissions.Pages_Builds_Approve,
                GameHubPermissions.Pages_Builds_Reject,
                GameHubPermissions.Pages_Moderation,
                GameHubPermissions.Pages_Moderation_View,
                GameHubPermissions.Pages_Moderation_Review,
                GameHubPermissions.Pages_Moderation_Complete,
                GameHubPermissions.Pages_Gameplay,
                GameHubPermissions.Pages_Leaderboard
            };
        }

        private static IEnumerable<string> DeveloperPermissions()
        {
            return new[]
            {
                GameHubPermissions.Pages_Games,
                GameHubPermissions.Pages_Games_View,
                GameHubPermissions.Pages_Games_Create,
                GameHubPermissions.Pages_Games_Edit,
                GameHubPermissions.Pages_Games_Delete,
                GameHubPermissions.Pages_Builds,
                GameHubPermissions.Pages_Builds_Upload,
                GameHubPermissions.Pages_Builds_View,
                GameHubPermissions.Pages_Gameplay,
                GameHubPermissions.Pages_Leaderboard
            };
        }

        private static IEnumerable<string> PlayerPermissions()
        {
            return new[]
            {
                GameHubPermissions.Pages_Games_View,
                GameHubPermissions.Pages_Gameplay,
                GameHubPermissions.Pages_Leaderboard
            };
        }
    }
}
