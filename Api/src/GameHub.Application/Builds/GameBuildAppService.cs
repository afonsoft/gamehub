using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Developer.Dto;
using GameHub.Storage;
using Microsoft.EntityFrameworkCore;

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

        [AbpAuthorize(GameHubPermissions.Pages_Builds_Upload)]
        public async Task<UploadGameBuildResultDto> UploadBuildAsync(Guid gameId, Stream packageStream, string fileName, string contentType)
        {
            var game = await _gameRepository.GetAsync(gameId);

            if (packageStream.Length > GameHubConsts.MaxBuildPackageSizeBytes)
            {
                return BuildFailedResult(fileName, $"Package exceeds maximum size of {GameHubConsts.MaxBuildPackageSizeBytes} bytes.");
            }

            var validation = await _validator.ValidateAsync(packageStream);

            if (!validation.IsValid)
            {
                return BuildFailedResult(fileName, string.Join("; ", validation.Errors), validation);
            }

            var buildId = Guid.NewGuid();
            var buildNumber = await GetNextBuildNumberAsync(gameId);
            var version = $"1.0.{buildNumber}";

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
                version,
                buildNumber,
                asset.Url,
                validation.PackageSizeBytes,
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
                Version = version,
                Status = build.Status.ToString(),
                ValidationSummary = validation
            };
        }

        private async Task<int> GetNextBuildNumberAsync(Guid gameId)
        {
            var max = await _buildRepository.GetAll()
                .Where(b => b.GameId == gameId)
                .Select(b => b.BuildNumber)
                .DefaultIfEmpty()
                .MaxAsync();

            return max + 1;
        }

        private static UploadGameBuildResultDto BuildFailedResult(string fileName, string error, ValidationSummaryDto validation = null)
        {
            var summary = validation ?? new ValidationSummaryDto();
            summary.IsValid = false;

            if (!summary.Errors.Contains(error))
            {
                summary.Errors.Add(error);
            }

            return new UploadGameBuildResultDto
            {
                BuildId = Guid.Empty,
                Version = fileName,
                Status = GameBuildStatus.ValidationFailed.ToString(),
                ValidationSummary = summary
            };
        }
    }
}
