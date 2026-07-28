using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Dependency;
using Eaf.Middleware.Authorization.Roles;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.EntityFrameworkCore;
using GameHub.Migrations.Seed.Host;
using GameHub.Migrations.Seed.Tenants;
using GameHub.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Transactions;

namespace GameHub.Migrations.Seed
{
    public static class SeedHelper
    {
        public static void SeedHostDb(IIocResolver iocResolver)
        {
            WithDbContext<GameHubDbContext>(iocResolver, SeedHostDb);
        }

        public static void SeedHostDb(GameHubDbContext context)
        {
            context.SuppressAutoSetTenantId = true;

            //Default tenant seed (in host database).
            new DefaultTenantBuilder(context).Create();
            new TenantRoleAndUserBuilder(context, 1).Create();

            //Player tenant seed (in host database).
            new PlayerTenantBuilder(context).Create();

            //Host seed
            new InitialHostDbBuilder(context).Create();

            //GameHub roles and permissions
            new GameHubPermissionSeeder(context).Create();

            LinkHostAdminToDefaultTenant(context);
        }

        public static void LinkHostAdminToDefaultTenant(GameHubDbContext context)
        {
            var hostAdmin = context.Users.IgnoreQueryFilters()
                .FirstOrDefault(u => u.TenantId == null && u.UserName == AbpUserBase.AdminUserName);
            var defaultTenant = context.Tenants.IgnoreQueryFilters()
                .FirstOrDefault(t => t.TenancyName == AbpTenantBase.DefaultTenantName);

            if (hostAdmin == null || defaultTenant == null)
                return;

            var existingMembership = context.UserTenantMemberships.IgnoreQueryFilters()
                .FirstOrDefault(m => m.UserId == hostAdmin.Id && m.TenantId == defaultTenant.Id);

            if (existingMembership != null)
                return;

            var tenantAdmin = context.Users.IgnoreQueryFilters()
                .FirstOrDefault(u => u.TenantId == defaultTenant.Id && u.UserName == AbpUserBase.AdminUserName);

            if (tenantAdmin != null)
            {
                tenantAdmin.Password = hostAdmin.Password;
                tenantAdmin.SecurityStamp = hostAdmin.SecurityStamp;
                tenantAdmin.SetNormalizedNames();
                context.Users.Update(tenantAdmin);
            }
            else
            {
                tenantAdmin = new User
                {
                    TenantId = defaultTenant.Id,
                    UserName = hostAdmin.UserName,
                    Name = hostAdmin.Name,
                    Surname = hostAdmin.Surname,
                    EmailAddress = hostAdmin.EmailAddress,
                    IsEmailConfirmed = hostAdmin.IsEmailConfirmed,
                    IsActive = hostAdmin.IsActive,
                    Password = hostAdmin.Password,
                    SecurityStamp = hostAdmin.SecurityStamp,
                };

                tenantAdmin.SetNormalizedNames();

                context.Users.Add(tenantAdmin);
                context.SaveChanges();

                // Reuse the tenant admin role assignment if it exists.
                var adminRole = context.Roles.IgnoreQueryFilters()
                    .FirstOrDefault(r => r.TenantId == defaultTenant.Id && r.Name == StaticRoleNames.Tenants.Admin);

                if (adminRole != null && !context.UserRoles.IgnoreQueryFilters().Any(ur => ur.UserId == tenantAdmin.Id && ur.RoleId == adminRole.Id))
                {
                    context.UserRoles.Add(new UserRole(defaultTenant.Id, tenantAdmin.Id, adminRole.Id));
                }
            }

            context.UserTenantMemberships.Add(new GameHub.MultiTenancy.UserTenantMembership
            {
                UserId = hostAdmin.Id,
                TenantId = defaultTenant.Id,
                TenantUserId = tenantAdmin.Id,
                IsDefault = true,
            });

            context.SaveChanges();
        }

        private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction) where TDbContext : DbContext
        {
            using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);
                contextAction(context);
                uow.Complete();
            }
        }
    }
}