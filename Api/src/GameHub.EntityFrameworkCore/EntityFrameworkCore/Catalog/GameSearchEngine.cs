using System;
using System.Linq;
using Abp.Dependency;
using Abp.EntityFrameworkCore;
using GameHub.Catalog;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace GameHub.EntityFrameworkCore.Catalog
{
    /// <summary>
    /// PostgreSQL full-text search implementation with a provider-safe fallback.
    /// </summary>
    public class GameSearchEngine : IGameSearchEngine, ITransientDependency
    {
        private readonly IDbContextProvider<GameHubDbContext> _dbContextProvider;

        public GameSearchEngine(IDbContextProvider<GameHubDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public IQueryable<Game> ApplySearchFilter(IQueryable<Game> query, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return query;
            }

            var dbContext = _dbContextProvider.GetDbContext();

            if (dbContext.Database.IsNpgsql())
            {
                var term = searchText.Trim();
                return query.Where(g => EF.Functions.ToTsVector("simple", g.Title + " " + g.ShortDescription)
                    .Matches(EF.Functions.PlainToTsQuery("simple", term)));
            }

            var normalized = searchText.ToLowerInvariant();
            return query.Where(g => g.Title.ToLower().Contains(normalized)
                || g.ShortDescription.ToLower().Contains(normalized));
        }
    }
}
