using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Authorization;
using GameHub.Builds.Dto;
using GameHub.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Builds
{
    /// <summary>
    /// Retrieves persisted build validation reports for moderators and admins.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Builds_View)]
    public class BuildValidationAppService : GameHubAppServiceBase, IBuildValidationAppService
    {
        private readonly IRepository<BuildValidationReport, Guid> _reportRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public BuildValidationAppService(
            IRepository<BuildValidationReport, Guid> reportRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<Game, Guid> gameRepository)
        {
            _reportRepository = reportRepository;
            _buildRepository = buildRepository;
            _gameRepository = gameRepository;
        }

        public async Task<BuildValidationReportDto> GetReportAsync(Guid gameBuildId)
        {
            var report = await _reportRepository.GetAll()
                .Where(r => r.GameBuildId == gameBuildId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (report == null)
            {
                return null;
            }

            return new BuildValidationReportDto
            {
                Id = report.Id,
                GameBuildId = report.GameBuildId,
                IsValid = report.IsValid,
                Errors = DeserializeList(report.ErrorsJson),
                Warnings = DeserializeList(report.WarningsJson),
                CreatedAt = report.CreatedAt
            };
        }

        public async Task<List<BuildValidationReportListItemDto>> GetReportsAsync(int? maxResultCount = 50)
        {
            var limit = maxResultCount ?? 50;

            var reports = await (from report in _reportRepository.GetAll()
                                 join build in _buildRepository.GetAll() on report.GameBuildId equals build.Id
                                 join game in _gameRepository.GetAll() on build.GameId equals game.Id
                                 orderby report.CreationTime descending
                                 select new
                                 {
                                     report.Id,
                                     report.GameBuildId,
                                     GameTitle = game.Title,
                                     Version = build.Version,
                                     report.IsValid,
                                     report.WarningsJson,
                                     report.ErrorsJson,
                                     report.CreatedAt
                                 })
                                 .Take(limit)
                                 .ToListAsync();

            return reports.Select(r => new BuildValidationReportListItemDto
            {
                Id = r.Id,
                GameBuildId = r.GameBuildId,
                GameTitle = r.GameTitle,
                Version = r.Version,
                IsValid = r.IsValid,
                WarningsCount = string.IsNullOrEmpty(r.WarningsJson) ? 0 : DeserializeList(r.WarningsJson).Count,
                ErrorsCount = string.IsNullOrEmpty(r.ErrorsJson) ? 0 : DeserializeList(r.ErrorsJson).Count,
                Warnings = string.IsNullOrEmpty(r.WarningsJson) ? new List<string>() : DeserializeList(r.WarningsJson),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        private static List<string> DeserializeList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }
    }
}
