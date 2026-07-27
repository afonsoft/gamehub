using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Builds;
using GameHub.Catalog;
using GameHub.Exceptions;
using GameHub.Developer.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    public class ModerationAppService : GameHubAppServiceBase, IModerationAppService
    {
        private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromMinutes(5);

        private readonly IRepository<ModerationReview, Guid> _reviewRepository;
        private readonly IRepository<UserReport, Guid> _reportRepository;
        private readonly IRepository<GameBuild, Guid> _buildRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IGameCatalogCache _catalogCache;
        private readonly ITypedCache<string, string> _idempotencyCache;

        public ModerationAppService(
            IRepository<ModerationReview, Guid> reviewRepository,
            IRepository<UserReport, Guid> reportRepository,
            IRepository<GameBuild, Guid> buildRepository,
            IRepository<Game, Guid> gameRepository,
            IGameCatalogCache catalogCache,
            ICacheManager cacheManager)
        {
            _reviewRepository = reviewRepository;
            _reportRepository = reportRepository;
            _buildRepository = buildRepository;
            _gameRepository = gameRepository;
            _catalogCache = catalogCache;
            _idempotencyCache = cacheManager
                .GetCache("GameHub.Moderation.ReviewIdempotency")
                .AsTyped<string, string>();
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<ListResultDto<ModerationReviewDto>> GetPendingReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAll()
                .Where(r => r.Status == ModerationReviewStatus.Pending && r.TenantId == AbpSession.TenantId)
                .Include(r => r.Game)
                .ToListAsync();

            return new ListResultDto<ModerationReviewDto>(ObjectMapper.Map<List<ModerationReviewDto>>(reviews));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_View)]
        public async Task<ModerationReviewDto> GetDetailAsync(Guid reviewId)
        {
            var review = await _reviewRepository.GetAll()
                .Where(r => r.Id == reviewId && r.TenantId == AbpSession.TenantId)
                .Include(r => r.Game)
                    .ThenInclude(g => g.GameBuilds)
                .Include(r => r.Game)
                    .ThenInclude(g => g.ModerationReviews)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Revisão não encontrada para o tenant atual.",
                    retryable: false);
            }

            var build = review.GameBuildId == Guid.Empty
                ? null
                : review.Game?.GameBuilds?.FirstOrDefault(b => b.Id == review.GameBuildId);

            var dto = ObjectMapper.Map<ModerationReviewDto>(review);
            dto.Version = build?.Version ?? string.Empty;
            dto.ValidationSummary = DeserializeValidationSummary(build?.ValidationSummary);
            dto.History = ObjectMapper.Map<List<ModerationReviewHistoryItemDto>>(
                (review.Game?.ModerationReviews?.Where(r => r.Id != reviewId).OrderByDescending(r => r.CreationTime) ?? Enumerable.Empty<ModerationReview>()).ToList());

            return dto;
        }

        private static ValidationSummaryDto DeserializeValidationSummary(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ValidationSummaryDto>(json);
            }
            catch
            {
                return null;
            }
        }

        [AbpAuthorize(GameHubPermissions.Pages_Moderation_Complete)]
        public async Task<ModerationReviewDto> CompleteReviewAsync(CompleteReviewInput input)
        {
            await EnsureCompleteReviewIdempotencyAsync(input);

            var review = await _reviewRepository.GetAll()
                .Where(r => r.Id == input.ReviewId && r.TenantId == AbpSession.TenantId)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Revisão não encontrada para o tenant atual.",
                    retryable: false);
            }

            if (review.Status != ModerationReviewStatus.Pending)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "A revisão não está pendente.",
                    retryable: false);
            }

            review.Decision = (ModerationDecision)input.Decision;
            review.Notes = input.Notes;
            review.Status = ModerationReviewStatus.Completed;
            review.ReviewerUserId = AbpSession.UserId ?? 0;

            var build = await _buildRepository.GetAll()
                .Where(b => b.Id == review.GameBuildId && b.TenantId == AbpSession.TenantId)
                .FirstOrDefaultAsync();

            if (build == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Build não encontrado para o tenant atual.",
                    retryable: false);
            }

            var game = await _gameRepository.GetAll()
                .Where(g => g.Id == review.GameId && g.TenantId == AbpSession.TenantId)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Jogo não encontrado para o tenant atual.",
                    retryable: false);
            }

            switch (review.Decision)
            {
                case ModerationDecision.Approved:
                    build.Publish();
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
                    throw new GameHubException(
                        GameHubErrorCodes.ValidationFailed,
                        $"Decisão de moderação não suportada: {review.Decision}.",
                        retryable: false);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            await _catalogCache.InvalidateHomeAsync();
            await _catalogCache.InvalidateBySlugAsync(game.Slug);

            return ObjectMapper.Map<ModerationReviewDto>(review);
        }

        private async Task EnsureCompleteReviewIdempotencyAsync(CompleteReviewInput input)
        {
            if (string.IsNullOrWhiteSpace(input.ClientRequestId))
                return;

            var key = $"gamehub:moderation:review:{input.ReviewId:N}:{input.ClientRequestId.Trim()}";
            var existing = await _idempotencyCache.GetOrDefaultAsync(key);
            if (existing != null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "Decisão de moderação já registrada para este identificador.",
                    retryable: false);
            }

            await _idempotencyCache.SetAsync(
                key,
                "1",
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(IdempotencyWindow));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Reports_Manage)]
        public async Task<ListResultDto<UserReportDto>> GetReportsAsync()
        {
            var reports = await _reportRepository.GetAll()
                .Where(r => r.TenantId == AbpSession.TenantId)
                .ToListAsync();
            return new ListResultDto<UserReportDto>(ObjectMapper.Map<List<UserReportDto>>(reports));
        }

        [AbpAuthorize(GameHubPermissions.Pages_Reports_Manage)]
        public async Task UpdateReportStatusAsync(Guid reportId, UserReportStatus status)
        {
            var report = await _reportRepository.GetAll()
                .Where(r => r.Id == reportId && r.TenantId == AbpSession.TenantId)
                .FirstOrDefaultAsync();

            if (report == null)
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "Report não encontrado para o tenant atual.",
                    retryable: false);
            }

            report.Status = status;
            report.ResolvedAt = status == UserReportStatus.Resolved ||
                status == UserReportStatus.Dismissed
                ? DateTime.UtcNow
                : null;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
