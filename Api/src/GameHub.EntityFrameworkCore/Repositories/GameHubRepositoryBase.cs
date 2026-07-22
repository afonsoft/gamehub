using Abp.Domain.Entities;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;
using GameHub.EntityFrameworkCore;

namespace GameHub.Repositories
{
    public abstract class GameHubRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<GameHubDbContext, TEntity, TPrimaryKey>
          where TEntity : class, IEntity<TPrimaryKey>
    {
        protected GameHubRepositoryBase(IDbContextProvider<GameHubDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }

    public abstract class GameHubRepositoryBase<TEntity> : GameHubRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected GameHubRepositoryBase(IDbContextProvider<GameHubDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}