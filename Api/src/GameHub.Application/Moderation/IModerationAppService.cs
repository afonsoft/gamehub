using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Moderation
{
    public interface IModerationAppService : IApplicationService
    {
        Task<ListResultDto<ModerationReviewDto>> GetPendingReviewsAsync();

        Task<ModerationReviewDto> GetDetailAsync(Guid reviewId);

        Task<ModerationReviewDto> CompleteReviewAsync(CompleteReviewInput input);

        Task<ListResultDto<UserReportDto>> GetReportsAsync();

        Task UpdateReportStatusAsync(Guid reportId, UserReportStatus status);
    }
}
