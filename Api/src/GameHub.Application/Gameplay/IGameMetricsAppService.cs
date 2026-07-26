using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Gameplay.Dto;

namespace GameHub.Gameplay
{
    public interface IGameMetricsAppService : IApplicationService
    {
        Task<GameMetricsResult> GetMetricsAsync(Guid gameId, GameMetricsFilter input);

        Task<GameMetricsExportDto> ExportCsvAsync(Guid gameId, GameMetricsFilter input);
    }
}
