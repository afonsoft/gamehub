using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;

namespace GameHub.EntityFrameworkCore
{
    public class GameHubDbMigrator : AbpZeroDbMigrator<GameHubDbContext>
    {
        public GameHubDbMigrator(
           IUnitOfWorkManager unitOfWorkManager, IDbPerTenantConnectionStringResolver connectionStringResolver, IDbContextResolver dbContextResolver
        ) : base(
            unitOfWorkManager,
            connectionStringResolver,
            dbContextResolver
        )
        { }
    }
}