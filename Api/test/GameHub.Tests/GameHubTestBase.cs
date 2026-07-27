using Abp;
using Abp.Authorization.Users;
using Abp.Events.Bus;
using Abp.Events.Bus.Entities;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.TestBase;
using Eaf.Middleware.Authorization.Roles;
using Eaf.Middleware.Authorization.Users;
using Eaf.Middleware.MultiTenancy;
using GameHub.EntityFrameworkCore;
using GameHub.Migrations.Seed;
using GameHub.Migrations.Seed.Host;
using GameHub.Migrations.Seed.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameHub.Tests
{
    public abstract class GameHubTestBase : AbpIntegratedTestBase<GameHubTestModule>
    {
        protected GameHubTestBase()
        {
            void NormalizeDbContext(GameHubDbContext context)
            {
                context.EntityChangeEventHelper = NullEntityChangeEventHelper.Instance;
                context.EventBus = NullEventBus.Instance;
                context.SuppressAutoSetTenantId = true;
            }

            // Seed initial data for host
            AbpSession.TenantId = null;
            UsingDbContext(context =>
            {
                NormalizeDbContext(context);
                new InitialHostDbBuilder(context).Create();
                new DefaultTenantBuilder(context).Create();
            });

            // Seed initial data for default tenant
            AbpSession.TenantId = 1;
            UsingDbContext(context =>
            {
                NormalizeDbContext(context);
                new TenantRoleAndUserBuilder(context, 1).Create();
            });

            LoginAsDefaultTenantAdmin();

            // Seed GameHub roles and permissions
            UsingDbContext(null, context =>
            {
                NormalizeDbContext(context);
                new GameHubPermissionSeeder(context).Create();
            });

            // Link host admin to the default tenant so multi-tenancy login tests work.
            UsingDbContext(null, context =>
            {
                NormalizeDbContext(context);
                SeedHelper.LinkHostAdminToDefaultTenant(context);
            });
        }

        #region UsingDbContext

        protected IDisposable UsingTenantId(int? tenantId)
        {
            var previousTenantId = AbpSession.TenantId;
            AbpSession.TenantId = tenantId;
            return new DisposeAction(() => AbpSession.TenantId = previousTenantId);
        }

        protected void UsingDbContext(Action<GameHubDbContext> action)
        {
            UsingDbContext(AbpSession.TenantId, action);
        }

        protected Task UsingDbContextAsync(Func<GameHubDbContext, Task> action)
        {
            return UsingDbContextAsync(AbpSession.TenantId, action);
        }

        protected T UsingDbContext<T>(Func<GameHubDbContext, T> func)
        {
            return UsingDbContext(AbpSession.TenantId, func);
        }

        protected Task<T> UsingDbContextAsync<T>(Func<GameHubDbContext, Task<T>> func)
        {
            return UsingDbContextAsync(AbpSession.TenantId, func);
        }

        protected void UsingDbContext(int? tenantId, Action<GameHubDbContext> action)
        {
            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<GameHubDbContext>())
                {
                    action(context);
                    context.SaveChanges();
                }
            }
        }

        protected async Task UsingDbContextAsync(int? tenantId, Func<GameHubDbContext, Task> action)
        {
            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<GameHubDbContext>())
                {
                    await action(context);
                    await context.SaveChangesAsync();
                }
            }
        }

        protected T UsingDbContext<T>(int? tenantId, Func<GameHubDbContext, T> func)
        {
            T result;

            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<GameHubDbContext>())
                {
                    result = func(context);
                    context.SaveChanges();
                }
            }

            return result;
        }

        protected async Task<T> UsingDbContextAsync<T>(int? tenantId, Func<GameHubDbContext, Task<T>> func)
        {
            T result;

            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<GameHubDbContext>())
                {
                    result = await func(context);
                    await context.SaveChangesAsync();
                }
            }

            return result;
        }

        #endregion UsingDbContext

        #region Login

        protected void LoginAsHostAdmin()
        {
            LoginAsHost(AbpUserBase.AdminUserName);
        }

        protected void LoginAsDefaultTenantAdmin()
        {
            LoginAsTenant(AbpTenantBase.DefaultTenantName, AbpUserBase.AdminUserName);
        }

        protected void LoginAsHost(string userName)
        {
            AbpSession.TenantId = null;

            var user =
                UsingDbContext(
                    context =>
                        context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new InvalidOperationException("There is no user: " + userName + " for host.");
            }

            AbpSession.UserId = user.Id;
        }

        protected void LoginAsTenant(string tenancyName, string userName)
        {
            var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
            if (tenant == null)
            {
                throw new InvalidOperationException("There is no tenant: " + tenancyName);
            }

            AbpSession.TenantId = tenant.Id;

            var user =
                UsingDbContext(
                    context =>
                        context.Users.FirstOrDefault(u => u.TenantId == AbpSession.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new InvalidOperationException("There is no user: " + userName + " for tenant: " + tenancyName);
            }

            AbpSession.UserId = user.Id;
        }

        #endregion Login

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected async Task<User> GetCurrentUserAsync()
        {
            var userId = AbpSession.GetUserId();
            return await UsingDbContext(context => context.Users.SingleAsync(u => u.Id == userId));
        }

        /// <summary>
        /// Gets current tenant if <see cref="IAbpSession.TenantId"/> is not null.
        /// Throws exception if there is no current tenant.
        /// </summary>
        protected async Task<Tenant> GetCurrentTenantAsync()
        {
            var tenantId = AbpSession.GetTenantId();
            return await UsingDbContext(context => context.Tenants.SingleAsync(t => t.Id == tenantId));
        }

        protected async Task<Tenant> GetTenantAsync(string tenancyName)
        {
            return await UsingDbContext(context => context.Tenants.SingleAsync(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant> GetTenantOrNullAsync(string tenancyName)
        {
            return await UsingDbContext(context => context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == tenancyName));
        }

        protected async Task<User> GetUserByUserNameOrNullAsync(string userName, bool includeRoles = false)
        {
            return await UsingDbContext(context => context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.TenantId == AbpSession.TenantId));
        }

        protected async Task<User> GetUserByUserNameAsync(string userName, bool includeRoles = false)
        {
            return await UsingDbContext(context => context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.TenantId == AbpSession.TenantId));
        }

        protected async Task<Role> GetRoleAsync(string roleName)
        {
            return await UsingDbContext(context => context.Roles.SingleAsync(u => u.Name == roleName));
        }
    }
}