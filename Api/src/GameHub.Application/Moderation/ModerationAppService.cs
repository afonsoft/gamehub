using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    public class ModerationAppService : GameHubAppServiceBase, IModerationAppService
    {
        private readonly IRepository<ModerationReview, Guid> _reviewRepository;
        private readonly IRepository<UserReport, Guid> _reportRepository;

        public ModerationAppService(
            IRepository<ModerationReview, Guid> reviewRepository,
            IRepository<UserReport, Guid> reportRepository)
        {
            _reviewRepository = reviewRepository;
            _reportRepository = reportRepository;
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<ListResultDto<ModerationReviewDto>> GetPendingReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAll()
                .Where(r => r.Status == ModerationReviewStatus.Pending)
                .Include(r => r.Game)
                .ToListAsync();

            return new ListResultDto<ModerationReviewDto>(ObjectMapper.Map<List<ModerationReviewDto>>(reviews));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<ModerationReviewDto> GetDetailAsync(Guid reviewId)
        {
            var review = await _reviewRepository.GetAll()
                .Where(r => r.Id == reviewId)
                .Include(r => r.Game)
                .FirstOrDefaultAsync();

            return ObjectMapper.Map<ModerationReviewDto>(review);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Complete)]
        public async Task<ModerationReviewDto> CompleteReviewAsync(CompleteReviewInput input)
        {
            var review = await _reviewRepository.GetAsync(input.ReviewId);
            review.Decision = (ModerationDecision)(int)input.Decision;
            review.Notes = input.Notes;
            review.Status = ModerationReviewStatus.Completed;
            review.ReviewerUserId = AbpSession.UserId ?? 0;

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<ModerationReviewDto>(review);
        }

        public async Task<ListResultDto<UserReportDto>> GetReportsAsync()
        {
            var reports = await _reportRepository.GetAll().ToListAsync();
            return new ListResultDto<UserReportDto>(ObjectMapper.Map<List<UserReportDto>>(reports));
        }

        public async Task UpdateReportStatusAsync(Guid reportId, UserReportStatus status)
        {
            var report = await _reportRepository.GetAsync(reportId);
            report.Status = status;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
