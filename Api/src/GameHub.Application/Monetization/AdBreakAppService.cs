using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Configuration;
using GameHub.Gameplay;
using GameHub.Monetization.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GameHub.Monetization
{
    /// <summary>
    /// Orchestrates commercial and rewarded ad breaks via IAdProvider.
    /// </summary>
    public class AdBreakAppService : GameHubAppServiceBase, IAdBreakAppService
    {
        private readonly IAdProvider _adProvider;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IRepository<AdImpression, Guid> _adImpressionRepository;
        private readonly AdBreakOptions _adBreakOptions;

        public AdBreakAppService(
            IAdProvider adProvider,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<AdImpression, Guid> adImpressionRepository,
            IOptions<AdBreakOptions> adBreakOptions)
        {
            _adProvider = adProvider;
            _playSessionRepository = playSessionRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _adImpressionRepository = adImpressionRepository;
            _adBreakOptions = adBreakOptions?.Value ?? new AdBreakOptions();
        }

        public async Task<CommercialBreakResultDto> RequestCommercialBreakAsync(RequestAdBreakInput input)
        {
            if (!_adBreakOptions.IsEnabled)
            {
                return new CommercialBreakResultDto
                {
                    Completed = false,
                    AdBlocked = false,
                    ErrorMessage = "Ad breaks are disabled."
                };
            }

            var result = await _adProvider.ShowCommercialBreakAsync(input.GameId);

            if (result.Completed)
            {
                await RecordAdImpressionAsync(input.GameId, input.SessionId, "commercial", _adBreakOptions.Provider ?? "Fake", result);
                if (input.SessionId.HasValue)
                {
                    await IncrementBreakCountsAsync(input.GameId, input.SessionId.Value, commercial: true);
                }
            }

            return new CommercialBreakResultDto
            {
                Completed = result.Completed,
                AdBlocked = result.AdBlocked,
                DurationSeconds = result.AdDurationSeconds,
                ErrorMessage = result.ErrorMessage
            };
        }

        public async Task<RewardedBreakResultDto> RequestRewardedBreakAsync(RequestAdBreakInput input)
        {
            if (!_adBreakOptions.IsEnabled)
            {
                return new RewardedBreakResultDto
                {
                    Completed = false,
                    RewardGranted = false,
                    AdBlocked = false,
                    ErrorMessage = "Ad breaks are disabled."
                };
            }

            var result = await _adProvider.ShowRewardedBreakAsync(input.GameId);
            var rewardGranted = result.Completed && result.RewardGranted && !result.AdBlocked;

            if (rewardGranted)
            {
                await RecordAdImpressionAsync(input.GameId, input.SessionId, "rewarded", _adBreakOptions.Provider ?? "Fake", result);
                if (input.SessionId.HasValue)
                {
                    await IncrementBreakCountsAsync(input.GameId, input.SessionId.Value, rewarded: true);
                }
            }

            return new RewardedBreakResultDto
            {
                Completed = result.Completed,
                RewardGranted = rewardGranted,
                AdBlocked = result.AdBlocked,
                ErrorMessage = result.ErrorMessage
            };
        }

        private async Task RecordAdImpressionAsync(Guid gameId, Guid? sessionId, string type, string provider, AdBreakResult result)
        {
            string countryCode = null;
            string deviceType = null;

            if (sessionId.HasValue)
            {
                var session = await _playSessionRepository.FirstOrDefaultAsync(s => s.Id == sessionId.Value);
                if (session != null)
                {
                    countryCode = session.CountryCode;
                    deviceType = session.DeviceType;
                }
            }

            var cpm = result.Cpm > 0 ? result.Cpm : (result.Earnings > 0 ? result.Earnings * 1000 : 0);

            await _adImpressionRepository.InsertAsync(new AdImpression
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                GameId = gameId,
                Type = type,
                Provider = provider,
                CountryCode = countryCode,
                DeviceType = deviceType,
                Cpm = cpm,
                Earnings = result.Earnings,
                OccurredAt = Clock.Now
            });
        }

        private async Task IncrementBreakCountsAsync(Guid gameId, Guid sessionId, bool commercial = false, bool rewarded = false)
        {
            var session = await _playSessionRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session != null)
            {
                if (commercial) session.CommercialBreakCount++;
                if (rewarded) session.RewardedBreakCount++;
            }

            var today = Clock.Now.Date;
            var metric = (await _metricSnapshotRepository.GetAll()
                .Where(m => m.GameId == gameId && m.Date.Year == today.Year && m.Date.Month == today.Month && m.Date.Day == today.Day)
                .ToListAsync())
                .FirstOrDefault();

            if (metric == null)
            {
                metric = new GameMetricSnapshot
                {
                    Id = Guid.NewGuid(),
                    TenantId = AbpSession.TenantId,
                    GameId = gameId,
                    Date = today
                };
                await _metricSnapshotRepository.InsertAsync(metric);
            }

            if (commercial) metric.CommercialBreakCount++;
            if (rewarded) metric.RewardedBreakCount++;

            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
