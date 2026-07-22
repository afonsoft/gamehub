using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using GameHub.Catalog;
using GameHub.Developer.Dto;
using GameHub.Storage;

namespace GameHub.Builds
{
    public class GameBuildAppService : GameHubAppServiceBase, IGameBuildAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IGameBuildPackageValidator _validator;
        private readonly IGameAssetStorage _assetStorage;

        public GameBuildAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IGameBuildPackageValidator validator,
            IGameAssetStorage assetStorage)
        {
            _gameRepository = gameRepository;
            _buildRepository = buildRepository;
            _validator = validator;
            _assetStorage = assetStorage;
        }

        public async Task<UploadGameBuildResultDto> UploadBuildAsync(Guid gameId, Stream packageStream, string fileName, string contentType)
        {
            var game = await _gameRepository.GetAsync(gameId);

            if (packageStream.Length > GameHubConsts.MaxBuildPackageSizeBytes)
            {
                return new UploadGameBuildResultDto
                {
                    BuildId = Guid.Empty,
                    Version = fileName,
                    Status = GameBuildStatus.ValidationFailed.ToString(),
                    ValidationSummary = $"Package exceeds maximum size of {GameHubConsts.MaxBuildPackageSizeBytes} bytes."
                };
            }

            var validation = await _validator.ValidateAsync(packageStream);

            if (!validation.IsValid)
            {
                return new UploadGameBuildResultDto
                {
                    BuildId = Guid.Empty,
                    Version = fileName,
                    Status = GameBuildStatus.ValidationFailed.ToString(),
                    ValidationSummary = string.Join("; ", validation.Errors)
                };
            }

            var buildId = Guid.NewGuid();
            var buildNumber = (await _buildRepository.CountAsync(b => b.GameId == gameId)) + 1;

            packageStream.Position = 0;
            var asset = await _assetStorage.StoreAsync(new GameBuildPackage
            {
                GameId = gameId,
                BuildId = buildId,
                FileName = fileName,
                ContentType = contentType,
                Content = packageStream
            });

            var build = new GameBuild(
                buildId,
                gameId,
                fileName,
                buildNumber,
                asset.Url,
                validation.SizeBytes,
                validation.HashSha256)
            {
                TenantId = AbpSession.TenantId,
                PublicBaseUrl = asset.PublicBaseUrl,
                IndexHtmlPath = validation.IndexHtmlPath,
                Status = GameBuildStatus.Validated
            };

            await _buildRepository.InsertAsync(build);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new UploadGameBuildResultDto
            {
                BuildId = buildId,
                Version = fileName,
                Status = build.Status.ToString(),
                ValidationSummary = string.Join("; ", validation.Errors)
            };
        }
    }
}
