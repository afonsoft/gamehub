using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using GameHub;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Catalog.Dto;
using GameHub.Developer.Dto;
using GameHub.Developers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    public class DeveloperGameAppService : GameHubAppServiceBase, IDeveloperGameAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly IGameCatalogCache _catalogCache;

        public DeveloperGameAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IGameCatalogCache catalogCache)
        {
            _gameRepository = gameRepository;
            _developerProfileRepository = developerProfileRepository;
            _catalogCache = catalogCache;
        }

        public async Task<GameDetailDto> CreateDraftAsync(CreateGameDraftInput input)
        {
            var profile = await GetOrCreateProfileAsync();

            var id = Guid.NewGuid();
            var game = new Game(
                id,
                input.Title,
                Slug.Create(input.Title).Value,
                input.ShortDescription,
                profile.Id);

            ObjectMapper.Map(input, game);

            await _gameRepository.InsertAsync(game);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<GameDetailDto>(game);
        }

        public async Task<GameDetailDto> UpdateMetadataAsync(UpdateGameMetadataInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            ObjectMapper.Map(input, game);

            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<GameDetailDto>(game);
        }

        public async Task<GameDetailDto> SubmitForReviewAsync(SubmitGameForReviewInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);

            if (game.Status != GameStatus.Draft && game.Status != GameStatus.Rejected)
            {
                throw new InvalidOperationException($"Game cannot be submitted for review from status {game.Status}.");
            }

            game.Status = GameStatus.InReview;
            await _catalogCache.InvalidateHomeAsync();

            return ObjectMapper.Map<GameDetailDto>(game);
        }

        public async Task<PagedResultDto<GameSummaryDto>> GetMyGamesAsync(GetGamesInput input)
        {
            var profile = await GetOrCreateProfileAsync();

            var query = _gameRepository.GetAll()
                .Where(g => g.DeveloperProfileId == profile.Id && !g.IsDeleted)
                .Include(g => g.PublishedBuild);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(g => g.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            return new PagedResultDto<GameSummaryDto>(total, ObjectMapper.Map<List<GameSummaryDto>>(items));
        }

        public async Task<ListResultDto<BuildDto>> GetBuildsAsync(Guid gameId)
        {
            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == gameId && !g.IsDeleted)
                .Include(g => g.GameBuilds)
                .FirstAsync();

            return new ListResultDto<BuildDto>(ObjectMapper.Map<List<BuildDto>>(game.GameBuilds.ToList()));
        }

        private async Task<DeveloperProfile> GetOrCreateProfileAsync()
        {
            var userId = AbpSession.UserId ?? 0;
            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
            {
                return profile;
            }

            profile = new DeveloperProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisplayName = AbpSession.UserId?.ToString() ?? "anonymous",
                Status = DeveloperProfileStatus.Pending
            };

            await _developerProfileRepository.InsertAsync(profile);
            return profile;
        }
    }
}
