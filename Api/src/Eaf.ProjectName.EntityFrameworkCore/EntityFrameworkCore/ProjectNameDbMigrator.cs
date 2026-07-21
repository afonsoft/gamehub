using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;

namespace Eaf.ProjectName.EntityFrameworkCore
{
    public class ProjectNameDbMigrator : AbpZeroDbMigrator<ProjectNameDbContext>
    {
        public ProjectNameDbMigrator(
           IUnitOfWorkManager unitOfWorkManager, IDbPerTenantConnectionStringResolver connectionStringResolver, IDbContextResolver dbContextResolver
        ) : base(
            unitOfWorkManager,
            connectionStringResolver,
            dbContextResolver
        )
        { }
    }
}