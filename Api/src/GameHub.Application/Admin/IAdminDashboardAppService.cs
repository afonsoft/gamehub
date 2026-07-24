using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Contrato de serviço do dashboard administrativo.
    /// </summary>
    public interface IAdminDashboardAppService : IApplicationService
    {
        Task<AdminDashboardSummaryDto> GetSummaryAsync();

        Task<PlaysOverTimeResultDto> GetPlaysOverTimeAsync(int days);

        Task<ListResultDto<AdminBuildListItemDto>> GetRecentUploadsAsync(int count);

        Task<ListResultDto<AdminGameListItemDto>> GetRecentGamesAsync(int count);

        Task<ListResultDto<AdminGameListItemDto>> GetTopGamesAsync(int count);

        Task<ListResultDto<ModerationReviewDto>> GetPendingReviewsAsync(int count);

        Task<AdminMetricsSummaryDto> GetMetricsAsync(DateTime? startDate, DateTime? endDate);

        Task<List<AdminHealthAlertDto>> GetHealthAlertsAsync();
    }
}
