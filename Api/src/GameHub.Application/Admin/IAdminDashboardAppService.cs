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

        Task<AdminOnboardingInsightsDto> GetOnboardingInsightsAsync(Guid gameId, DateTime? startDate, DateTime? endDate);

        Task<AdminEngagementInsightsDto> GetEngagementInsightsAsync(Guid gameId, DateTime? startDate, DateTime? endDate);

        Task<ErrorScannerResultDto> GetErrorScannerAsync(Guid? gameId, Guid? buildId, int hours);

        Task<ConversionFunnelDto> GetConversionFunnelAsync(Guid? gameId, DateTime? startDate, DateTime? endDate);

        Task<PlayerFitDto> GetPlayerFitAsync(Guid gameId);

        Task<List<MultiplayerMetricsDto>> GetMultiplayerMetricsAsync(Guid? gameId, DateTime? startDate, DateTime? endDate);
    }
}
