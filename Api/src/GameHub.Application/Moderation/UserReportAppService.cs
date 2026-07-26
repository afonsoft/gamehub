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
using GameHub.Catalog;
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
        private const int MaxReportsPerMinute = 10;
        private readonly IRepository<UserReport, Guid> _userReportRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly ITypedCache<string, string> _rateLimitCache;

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
        }

        public async Task<UserReportDto> SubmitAsync(UserReportInput input)
        {
            await _gameRepository.GetAsync(input.GameId);
            await EnsureRateLimitAsync(input.GameId);

            var report = new UserReport
            {
                Id = Guid.NewGuid(),
                GameId = input.GameId,
                UserId = AbpSession.UserId,
                Reason = input.Reason,
                Description = input.Description,
                Status = UserReportStatus.Open
            };

            await _userReportRepository.InsertAsync(report);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<UserReportDto>(report);
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
                throw new InvalidOperationException(
                    "User report rate limit exceeded.");
            }

            await _rateLimitCache.SetAsync(
                key,
                (count + 1).ToString(),
                absoluteExpireTime: DateTimeOffset.UtcNow.Add(RateLimitWindow));
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
