using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using GameHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace GameHub.HealthCheck
{
    public class EafDbContextHealthCheck : IHealthCheck
    {
        private readonly IIocResolver _iocResolver;

        public EafDbContextHealthCheck(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var uowManager = _iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
                using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
                {
                    var dbContext = await uowManager.Object.Current.GetDbContextAsync<ProjectNameDbContext>(MultiTenancySides.Host);
                    await dbContext.Database.OpenConnectionAsync(cancellationToken);
                    await uow.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Error on test Connection DataBase", ex);
            }

            return HealthCheckResult.Healthy();
        }
    }
}