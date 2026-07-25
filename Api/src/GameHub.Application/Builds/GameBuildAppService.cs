using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Authorization;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Storage;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Builds
{
    public class GameBuildAppService : GameHubAppServiceBase, IGameBuildAppService
    {
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<BuildValidationReport, Guid> _reportRepository;
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly IRepository<DeveloperTeam, Guid> _developerTeamRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _developerTeamMemberRepository;
        private readonly IGameBuildPackageValidator _validator;
        private readonly IGameAssetStorage _assetStorage;

        public GameBuildAppService(
            IRepository<Game, Guid> gameRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<BuildValidationReport, Guid> reportRepository,
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IRepository<DeveloperTeam, Guid> developerTeamRepository,
            IRepository<DeveloperTeamMember, Guid> developerTeamMemberRepository,
            IGameBuildPackageValidator validator,
            IGameAssetStorage assetStorage)
        {
            _gameRepository = gameRepository;
            _buildRepository = buildRepository;
            _reportRepository = reportRepository;
            _developerProfileRepository = developerProfileRepository;
            _developerTeamRepository = developerTeamRepository;
            _developerTeamMemberRepository = developerTeamMemberRepository;
            _validator = validator;
            _assetStorage = assetStorage;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Builds_Upload)]
        public async Task<UploadGameBuildResultDto> UploadBuildAsync(Guid gameId, Stream packageStream, string fileName, string contentType)
        {
            var game = await _gameRepository.GetAsync(gameId);
            return await UploadBuildCoreAsync(game, packageStream, fileName, contentType, version: null);
        }

        [AbpAuthorize]
        public async Task<UploadGameBuildResultDto> UploadFromCliAsync(UploadFromCliInput input)
        {
            var game = await ResolveGameByApiKeyAsync(input.ApiKey, input.GameSlug);
            if (game == null)
            {
                return BuildFailedResult(input.GameSlug, "Invalid API key or game slug.");
            }

            using var packageStream = new MemoryStream(input.Package);
            return await UploadBuildCoreAsync(game, packageStream, $"{game.Slug}.zip", "application/zip", input.Version);
        }

        private async Task<UploadGameBuildResultDto> UploadBuildCoreAsync(Game game, Stream packageStream, string fileName, string contentType, string version)
        {
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
            var buildNumber = await GetNextBuildNumberAsync(game.Id);
            var resolvedVersion = string.IsNullOrWhiteSpace(version) ? $"1.0.{buildNumber}" : version;

            packageStream.Position = 0;
            var asset = await _assetStorage.StoreAsync(new GameBuildPackage
            {
                GameId = game.Id,
                BuildId = buildId,
                FileName = fileName,
                ContentType = contentType,
                Content = packageStream
            });

            var build = new GameBuild(
                buildId,
                game.Id,
                resolvedVersion,
                buildNumber,
                asset.Url,
                validation.PackageSizeBytes,
                validation.HashSha256)
            {
                TenantId = game.TenantId,
                PublicBaseUrl = asset.PublicBaseUrl,
                IndexHtmlPath = validation.IndexHtmlPath,
                Status = GameBuildStatus.Validated
            };

            await _buildRepository.InsertAsync(build);
            await SaveValidationReportAsync(buildId, validation);
            await CurrentUnitOfWork.SaveChangesAsync();

            return new UploadGameBuildResultDto
            {
                BuildId = buildId,
                Version = resolvedVersion,
                Status = build.Status.ToString(),
                ValidationSummary = validation
            };
        }

        private async Task<Game> ResolveGameByApiKeyAsync(string apiKey, string gameSlug)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(gameSlug))
            {
                return null;
            }

            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.ApiKey == apiKey);
            if (profile != null)
            {
                return await _gameRepository.FirstOrDefaultAsync(g => g.Slug == gameSlug && g.DeveloperProfileId == profile.Id);
            }

            var team = await _developerTeamRepository.FirstOrDefaultAsync(t => t.ApiKey == apiKey);
            if (team == null)
            {
                return null;
            }

            var member = await _developerTeamMemberRepository.FirstOrDefaultAsync(m => m.TeamId == team.Id && (m.Role == DeveloperTeamRole.Developer || m.Role == DeveloperTeamRole.Billing));
            if (member == null)
            {
                return null;
            }

            profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == member.UserId);
            if (profile == null)
            {
                return null;
            }

            return await _gameRepository.FirstOrDefaultAsync(g => g.Slug == gameSlug && g.DeveloperProfileId == profile.Id);
        }

        private async Task SaveValidationReportAsync(Guid buildId, ValidationSummaryDto summary)
        {
            var report = new BuildValidationReport
            {
                Id = Guid.NewGuid(),
                GameBuildId = buildId,
                IsValid = summary.IsValid,
                HasExternalRequests = summary.HasExternalRequests,
                ErrorsJson = JsonSerializer.Serialize(summary.Errors ?? new System.Collections.Generic.List<string>()),
                WarningsJson = JsonSerializer.Serialize(summary.Warnings ?? new System.Collections.Generic.List<string>()),
                CreatedAt = DateTime.UtcNow,
                TenantId = AbpSession.TenantId
            };

            await _reportRepository.InsertAsync(report);
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
