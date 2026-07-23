using System.Linq;

namespace GameHub.Catalog
{
    /// <summary>
    /// Abstraction for applying a search filter over a game query in a provider-aware way.
    /// </summary>
    public interface IGameSearchEngine
    {
        /// <summary>
        /// Applies a full-text or fallback search filter to the given query.
        /// </summary>
        /// <param name="query">The base game query.</param>
        /// <param name="searchText">The search term.</param>
        /// <returns>A query filtered by the search term.</returns>
        IQueryable<Game> ApplySearchFilter(IQueryable<Game> query, string searchText);
    }
}
