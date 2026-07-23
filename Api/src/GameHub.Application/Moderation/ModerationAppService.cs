using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Builds;
using GameHub.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    public class ModerationAppService : GameHubAppServiceBase, IModerationAppService
    {
        private readonly IRepository<ModerationReview, Guid> _reviewRepository;
        private readonly IRepository<UserReport, Guid> _reportRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;

        public ModerationAppService(
            IRepository<ModerationReview, Guid> reviewRepository,
            IRepository<UserReport, Guid> reportRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<Game, Guid> gameRepository)
        {
            _reviewRepository = reviewRepository;
            _reportRepository = reportRepository;
            _buildRepository = buildRepository;
            _gameRepository = gameRepository;
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

            if (review.Status != ModerationReviewStatus.Pending)
            {
                throw new UserFriendlyException("Review is not pending.");
            }

            review.Decision = (ModerationDecision)input.Decision;
            review.Notes = input.Notes;
            review.Status = ModerationReviewStatus.Completed;
            review.ReviewerUserId = AbpSession.UserId ?? 0;

            var build = await _buildRepository.GetAsync(review.GameBuildId);
            var game = await _gameRepository.GetAsync(review.GameId);

            switch (review.Decision)
            {
                case ModerationDecision.Approved:
                    build.Approve();
                    game.Publish(build.Id);
                    break;
                case ModerationDecision.Rejected:
                    build.Reject(input.Notes);
                    game.Status = GameStatus.Rejected;
                    break;
                case ModerationDecision.RequiresChanges:
                    game.Status = GameStatus.Draft;
                    break;
                default:
                    throw new UserFriendlyException($"Unsupported moderation decision {review.Decision}.");
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<ModerationReviewDto>(review);
        }

        [AbpAuthorize(GameHubPermissions.Pages_Reports_Manage)]
        public async Task<ListResultDto<UserReportDto>> GetReportsAsync()
        {
            var reports = await _reportRepository.GetAll().ToListAsync();
            return new ListResultDto<UserReportDto>(ObjectMapper.Map<List<UserReportDto>>(reports));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Reports_Manage)]
        public async Task UpdateReportStatusAsync(Guid reportId, UserReportStatus status)
        {
            var report = await _reportRepository.GetAsync(reportId);
            report.Status = status;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
