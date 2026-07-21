using Abp.Domain.Entities;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;
using Eaf.ProjectName.EntityFrameworkCore;

namespace Eaf.ProjectName.Repositories
{
    public abstract class ProjectNameRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<ProjectNameDbContext, TEntity, TPrimaryKey>
          where TEntity : class, IEntity<TPrimaryKey>
    {
        protected ProjectNameRepositoryBase(IDbContextProvider<ProjectNameDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }

    public abstract class ProjectNameRepositoryBase<TEntity> : ProjectNameRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected ProjectNameRepositoryBase(IDbContextProvider<ProjectNameDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}