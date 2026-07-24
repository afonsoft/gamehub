using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Security;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Builds
{
    public class GamePreviewAppService : GameHubAppServiceBase, IGamePreviewAppService
    {
        private readonly IRepository<PreviewToken, Guid> _previewTokenRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IGameTokenProvider _gameTokenProvider;

        public GamePreviewAppService(
            IRepository<PreviewToken, Guid> previewTokenRepository,
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IGameTokenProvider gameTokenProvider)
        {
            _previewTokenRepository = previewTokenRepository;
            _gameRepository = gameRepository;
            _buildRepository = buildRepository;
            _gameTokenProvider = gameTokenProvider;
        }

        [AbpAuthorize]
        public async Task<CreatePreviewTokenResult> CreatePreviewTokenAsync(CreatePreviewTokenInput input)
        {
            var game = await _gameRepository.GetAsync(input.GameId);
            var build = await _buildRepository.FirstOrDefaultAsync(
                b => b.GameId == input.GameId && b.Version == input.Version && !b.IsDeleted);

            if (build == null)
            {
                throw new UserFriendlyException($"Build version {input.Version} not found for this game.");
            }

            var userId = AbpSession.UserId ?? 0;
            var token = await _gameTokenProvider.CreatePreviewTokenAsync(
                userId,
                AbpSession.TenantId,
                input.GameId,
                input.Version,
                TimeSpan.FromHours(24));

            var previewToken = new PreviewToken
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                GameBuildId = build.Id,
                Version = input.Version,
                TokenValue = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedByUserId = userId
            };

            await _previewTokenRepository.InsertAsync(previewToken);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new CreatePreviewTokenResult
            {
                Token = token,
                PreviewUrl = $"/preview/{game.Slug}/{input.Version}?token={Uri.EscapeDataString(token)}",
                Version = input.Version,
                GameSlug = game.Slug
            };
        }

        [AbpAllowAnonymous]
        public async Task<ValidatePreviewResult> ValidatePreviewAsync(ValidatePreviewInput input)
        {
            var previewToken = await _previewTokenRepository.FirstOrDefaultAsync(
                t => t.TokenValue == input.Token && t.ExpiresAt > DateTime.UtcNow);

            if (previewToken == null)
            {
                return new ValidatePreviewResult { IsValid = false, Error = "Invalid or expired preview token." };
            }

            var build = await _buildRepository.GetAsync(previewToken.GameBuildId);
            var previewUrl = BuildPreviewUrl(build);

            if (string.IsNullOrWhiteSpace(previewUrl))
            {
                return new ValidatePreviewResult { IsValid = false, Error = "Build is not available for preview." };
            }

            return new ValidatePreviewResult { IsValid = true, PreviewUrl = previewUrl };
        }

        private static string BuildPreviewUrl(GameBuild build)
        {
            if (string.IsNullOrWhiteSpace(build.PublicBaseUrl))
            {
                return null;
            }

            var baseUrl = build.PublicBaseUrl.TrimEnd('/');
            var indexPath = string.IsNullOrWhiteSpace(build.IndexHtmlPath) ? "index.html" : build.IndexHtmlPath.TrimStart('/');
            return $"{baseUrl}/{indexPath}";
        }
    }
}
