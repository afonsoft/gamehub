using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Catalog.Dto;
using GameHub.Player.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Player
{
    /// <summary>
    /// Implements player favorites and recent game tracking.
    /// </summary>
    public class PlayerAccountAppService : GameHubAppServiceBase, IPlayerAccountAppService
    {
        private readonly IRepository<PlayerFavorite, Guid> _favoriteRepository;
        private readonly IRepository<PlayerRecentGame, Guid> _recentRepository;
        private readonly IRepository<Catalog.Game, Guid> _gameRepository;

        public PlayerAccountAppService(
            IRepository<PlayerFavorite, Guid> favoriteRepository,
            IRepository<PlayerRecentGame, Guid> recentRepository,
            IRepository<Catalog.Game, Guid> gameRepository)
        {
            _favoriteRepository = favoriteRepository;
            _recentRepository = recentRepository;
            _gameRepository = gameRepository;
        }

        public async Task<List<PlayerFavoriteDto>> GetFavoritesAsync()
        {
            var userId = AbpSession.UserId;
            if (!userId.HasValue)
            {
                return new List<PlayerFavoriteDto>();
            }

            var favorites = await _favoriteRepository.GetAll()
                .Where(f => f.UserId == userId.Value)
                .OrderByDescending(f => f.CreationTime)
                .Include(f => f.Game)
                    .ThenInclude(g => g.GameCategories)
                        .ThenInclude(gc => gc.Category)
                .Include(f => f.Game.DeveloperProfile)
                .Include(f => f.Game.PublishedBuild)
                .Include(f => f.Game.RevenueContracts)
                .ToListAsync();

            return favorites.Select(MapFavorite).ToList();
        }

        public async Task<bool> ToggleFavoriteAsync(ToggleFavoriteInput input)
        {
            var userId = AbpSession.UserId;
            if (!userId.HasValue)
            {
                return false;
            }

            var existing = await _favoriteRepository.FirstOrDefaultAsync(
                f => f.GameId == input.GameId && f.UserId == userId.Value);

            if (existing != null)
            {
                await _favoriteRepository.DeleteAsync(existing);
                return false;
            }

            await _favoriteRepository.InsertAsync(new PlayerFavorite
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = userId.Value,
                CreationTime = Clock.Now
            });

            await CurrentUnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlayerRecentGameDto>> GetRecentAsync(GetRecentInput input)
        {
            var userId = AbpSession.UserId;
            if (!userId.HasValue)
            {
                return new List<PlayerRecentGameDto>();
            }

            var recent = await _recentRepository.GetAll()
                .Where(r => r.UserId == userId.Value)
                .OrderByDescending(r => r.LastPlayedAt)
                .Take(input.Max)
                .Include(r => r.Game)
                    .ThenInclude(g => g.GameCategories)
                        .ThenInclude(gc => gc.Category)
                .Include(r => r.Game.DeveloperProfile)
                .Include(r => r.Game.PublishedBuild)
                .Include(r => r.Game.RevenueContracts)
                .ToListAsync();

            return recent.Select(MapRecent).ToList();
        }

        public async Task TrackPlayAsync(TrackPlayInput input)
        {
            var userId = AbpSession.UserId;
            if (!userId.HasValue)
            {
                return;
            }

            var existing = await _recentRepository.FirstOrDefaultAsync(
                r => r.GameId == input.GameId && r.UserId == userId.Value);

            if (existing != null)
            {
                existing.LastPlayedAt = Clock.Now;
                existing.TotalSessions++;
            }
            else
            {
                await _recentRepository.InsertAsync(new PlayerRecentGame(
                    Guid.NewGuid(), input.GameId, userId.Value)
                {
                    TenantId = AbpSession.TenantId,
                    LastPlayedAt = Clock.Now,
                    TotalSessions = 1
                });
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task MergeLocalDataAsync(MergePlayerDataInput input)
        {
            var userId = AbpSession.UserId;
            if (!userId.HasValue)
            {
                return;
            }

            foreach (var gameId in input.FavoriteGameIds ?? new List<Guid>())
            {
                var exists = await _favoriteRepository.FirstOrDefaultAsync(
                    f => f.GameId == gameId && f.UserId == userId.Value);

                if (exists == null)
                {
                    await _favoriteRepository.InsertAsync(new PlayerFavorite
                    {
                        Id = Guid.NewGuid(),
                        TenantId = AbpSession.TenantId,
                        GameId = gameId,
                        UserId = userId.Value,
                        CreationTime = Clock.Now
                    });
                }
            }

            foreach (var gameId in input.RecentGameIds ?? new List<Guid>())
            {
                await TrackPlayAsync(new TrackPlayInput { GameId = gameId });
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        private PlayerFavoriteDto MapFavorite(PlayerFavorite favorite)
        {
            return new PlayerFavoriteDto
            {
                GameId = favorite.GameId,
                Game = ObjectMapper.Map<GameCardDto>(favorite.Game),
                CreatedAt = favorite.CreationTime
            };
        }

        private PlayerRecentGameDto MapRecent(PlayerRecentGame recent)
        {
            return new PlayerRecentGameDto
            {
                GameId = recent.GameId,
                Game = ObjectMapper.Map<GameCardDto>(recent.Game),
                LastPlayedAt = recent.LastPlayedAt,
                TotalSessions = recent.TotalSessions
            };
        }
    }
}
