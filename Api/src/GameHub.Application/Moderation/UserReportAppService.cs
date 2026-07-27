using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Abp.Timing;
using GameHub.Admin.Dto;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Exceptions;
using GameHub.Moderation.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Moderation
{
    /// <summary>
    /// Permite que jogadores reportem jogos ou conteúdo inadequado.
    /// </summary>
    public class UserReportAppService : GameHubAppServiceBase, IUserReportAppService
    {
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromMinutes(5);
        private const int MaxReportsPerMinute = 10;

        private readonly IRepository<UserReport, Guid> _userReportRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly ITypedCache<string, string> _rateLimitCache;
        private readonly ITypedCache<string, string> _idempotencyCache;

        public UserReportAppService(
            IRepository<UserReport, Guid> userReportRepository,
            IRepository<Game, Guid> gameRepository,
            ICacheManager cacheManager)
        {
            _userReportRepository = userReportRepository;
            _gameRepository = gameRepository;
            _rateLimitCache = cacheManager
                .GetCache("GameHub.Moderation.UserReportRateLimit")
                .AsTyped<string, string>();
            _idempotencyCache = cacheManager
                .GetCache("GameHub.Moderation.UserReportIdempotency")
                .AsTyped<string, string>();
        }

        public async Task<UserReportDto> SubmitAsync(UserReportInput input)
        {
            await ValidateInputAsync(input);
            await EnsureRateLimitAsync(input.GameId);

            var idempotencyKey = BuildIdempotencyKey(input.GameId, input.ClientRequestId);
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingId = await _idempotencyCache.GetOrDefaultAsync(idempotencyKey);
                if (existingId != null)
                {
                    var existing = await _userReportRepository.GetAsync(Guid.Parse(existingId));
                    return ObjectMapper.Map<UserReportDto>(existing);
                }
            }

            var report = new UserReport
            {
                Id = SequentialGuidGenerator.Instance.Create(),
                TenantId = AbpSession.TenantId,
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                Reason = input.Reason.Trim(),
                Description = input.Description?.Trim() ?? string.Empty,
                Status = UserReportStatus.Open,
                CreationTime = Clock.Now
            };

            await _userReportRepository.InsertAsync(report);
            await CurrentUnitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                await _idempotencyCache.SetAsync(
                    idempotencyKey,
                    report.Id.ToString(),
                    absoluteExpireTime: DateTimeOffset.UtcNow.Add(IdempotencyWindow));
            }

            return ObjectMapper.Map<UserReportDto>(report);
        }

        private async Task ValidateInputAsync(UserReportInput input)
        {
            if (!AbpSession.UserId.HasValue)
            {
                throw new GameHubException(
                    GameHubErrorCodes.NotAuthenticated,
                    "É necessário estar autenticado para reportar.",
                    retryable: false);
            }

            if (!await _gameRepository.GetAll().AnyAsync(g => g.Id == input.GameId && !g.IsDeleted))
            {
                throw new GameHubException(
                    GameHubErrorCodes.InvalidContext,
                    "O jogo informado não foi encontrado.",
                    retryable: false);
            }

            if (string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length > 128)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "O motivo do report é obrigatório e deve ter até 128 caracteres.",
                    retryable: false);
            }

            if (!string.IsNullOrEmpty(input.Description) && input.Description.Length > 2000)
            {
                throw new GameHubException(
                    GameHubErrorCodes.ValidationFailed,
                    "A descrição do report deve ter até 2000 caracteres.",
                    retryable: false);
            }
        }

        private async Task EnsureRateLimitAsync(Guid gameId)
        {
            var key =
                $"gamehub:moderation:report:" +
                $"{AbpSession.TenantId?.ToString() ?? "host"}:" +
                $"{AbpSession.UserId?.ToString() ?? "anonymous"}:" +
                $"{gameId:N}";

            var current = await _rateLimitCache.GetOrDefaultAsync(key);
            var count = int.TryParse(current, out var parsed) ? parsed : 0;

            if (count >= MaxReportsPerMinute)
            {
                throw new GameHubException(
                    GameHubErrorCodes.RateLimited,
                    "Limite de envio de reports excedido. Tente novamente mais tarde.",
                    retryable: true);
            }

            await _rateLimitCache.SetAsync(
                key,
                (count + 1).ToString(),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(RateLimitWindow));
        }

        private static string BuildIdempotencyKey(Guid gameId, string clientRequestId)
        {
            if (string.IsNullOrWhiteSpace(clientRequestId))
                return null;

            return $"gamehub:moderation:report:idempotency:{gameId:N}:{clientRequestId.Trim()}";
        }

        [AbpAuthorize(GameHubPermissions.Pages_Reports_Manage)]
        public async Task<PagedResultDto<UserReportDto>> GetAllAsync(GetReportsInput input)
        {
            var query = _userReportRepository.GetAll().Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.Status) && Enum.TryParse<UserReportStatus>(input.Status, true, out var status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (input.GameId.HasValue)
            {
                query = query.Where(r => r.GameId == input.GameId.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Include(r => r.Game)
                .ToListAsync();

            return new PagedResultDto<UserReportDto>(total, ObjectMapper.Map<List<UserReportDto>>(items));
        }
    }
}
