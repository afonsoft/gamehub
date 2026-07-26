using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Developer.Dto;
using GameHub.Developers;
using GameHub.Gameplay;
using GameHub.Monetization;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Developer
{
    /// <summary>
    /// Computes estimated earnings for the authenticated developer.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
    public class DeveloperEarningsAppService : GameHubAppServiceBase, IDeveloperEarningsAppService
    {
        private readonly IRepository<DeveloperProfile, Guid> _developerProfileRepository;
        private readonly IRepository<Game, Guid> _gameRepository;
        private readonly IRepository<GameMetricSnapshot, Guid> _metricSnapshotRepository;
        private readonly IRepository<RevenueContract, Guid> _revenueContractRepository;
        private readonly IRepository<PlaySession, Guid> _playSessionRepository;
        private readonly IRepository<AdImpression, Guid> _adImpressionRepository;
        private readonly IRepository<DeveloperTeamMember, Guid> _teamMemberRepository;

        public DeveloperEarningsAppService(
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IRepository<Game, Guid> gameRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<RevenueContract, Guid> revenueContractRepository,
            IRepository<PlaySession, Guid> playSessionRepository,
            IRepository<AdImpression, Guid> adImpressionRepository,
            IRepository<DeveloperTeamMember, Guid> teamMemberRepository)
        {
            _developerProfileRepository = developerProfileRepository;
            _gameRepository = gameRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _revenueContractRepository = revenueContractRepository;
            _playSessionRepository = playSessionRepository;
            _adImpressionRepository = adImpressionRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        public async Task<DeveloperEarningsDto> GetEarningsAsync(GetDeveloperEarningsInput input)
        {
            await EnsureCurrentUserIsNotSupportAsync();
            ValidateDateRange(input);

            var to = (input.To ?? Clock.Now).Date.AddDays(1).AddTicks(-1);
            var from = (input.From ?? to.AddDays(-29)).Date;

            if (!AbpSession.UserId.HasValue)
            {
                throw new AbpAuthorizationException("User is not authenticated.");
            }

            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId.Value);
            if (profile == null)
            {
                return new DeveloperEarningsDto { From = from, To = to };
            }

            var games = await _gameRepository.GetAll()
                .Where(g => g.DeveloperProfileId == profile.Id && !g.IsDeleted)
                .OrderBy(g => g.Title)
                .Take(input.MaxResultCount)
                .ToListAsync();

            var gameIds = games.Select(g => g.Id).ToList();

            var snapshots = await _metricSnapshotRepository.GetAll()
                .Where(s => gameIds.Contains(s.GameId) && s.Date >= from && s.Date <= to)
                .ToListAsync();

            var contracts = await _revenueContractRepository.GetAll()
                .Where(c => gameIds.Contains(c.GameId) && c.IsActive)
                .GroupBy(c => c.GameId)
                .Select(g => g.OrderByDescending(c => c.EffectiveDate).First())
                .ToDictionaryAsync(c => c.GameId, c => c);

            var trafficShares = await GetTrafficSharesAsync(gameIds);

            var gameEarnings = new List<GameEarningsDto>();
            var gameById = games.ToDictionary(g => g.Id);
            var gameSnapshots = snapshots.GroupBy(s => s.GameId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var game in games)
            {
                var gameSnapshotList = gameSnapshots.GetValueOrDefault(game.Id) ?? new List<GameMetricSnapshot>();
                var contract = contracts.GetValueOrDefault(game.Id);
                var contractType = contract?.ContractType ?? RevenueContractType.NonExclusive;
                var flatFeeAmount = contract?.FlatFeeAmount ?? 0m;
                var developerShare = ComputeDeveloperShare(contractType, game.Id, trafficShares);

                var earnings = BuildGameEarnings(game, gameSnapshotList, contractType, flatFeeAmount, developerShare);
                gameEarnings.Add(earnings);
            }

            var totalGross = gameEarnings.Sum(g => g.GrossEstimatedRevenue);
            var totalDev = gameEarnings.Sum(g => g.DeveloperEstimatedRevenue);

            return new DeveloperEarningsDto
            {
                From = from,
                To = to,
                TotalGrossEstimatedRevenue = totalGross,
                TotalDeveloperEstimatedRevenue = totalDev,
                TotalPlatformEstimatedRevenue = totalGross - totalDev,
                TotalCommercialBreaks = gameEarnings.Sum(g => g.CommercialBreaks),
                TotalRewardedBreaks = gameEarnings.Sum(g => g.RewardedBreaks),
                Games = gameEarnings
            };
        }

        private GameEarningsDto BuildGameEarnings(Game game, List<GameMetricSnapshot> gameSnapshots, RevenueContractType contractType, decimal flatFeeAmount, decimal developerShare)
        {
            var daily = gameSnapshots
                .GroupBy(s => s.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var commercialBreaks = g.Sum(s => s.CommercialBreakCount);
                    var rewardedBreaks = g.Sum(s => s.RewardedBreakCount);
                    var gross = EstimateGrossRevenue(commercialBreaks, rewardedBreaks);
                    return new DailyEarningsDto
                    {
                        Date = g.Key,
                        CommercialBreaks = commercialBreaks,
                        RewardedBreaks = rewardedBreaks,
                        GrossEstimatedRevenue = gross,
                        DeveloperEstimatedRevenue = gross * developerShare
                    };
                })
                .ToList();

            var totalCommercialBreaks = daily.Sum(d => d.CommercialBreaks);
            var totalRewardedBreaks = daily.Sum(d => d.RewardedBreaks);
            var adGrossRevenue = daily.Sum(d => d.GrossEstimatedRevenue);
            var grossRevenue = contractType == RevenueContractType.NonExclusive
                ? adGrossRevenue + flatFeeAmount
                : adGrossRevenue;
            var developerRevenue = contractType == RevenueContractType.NonExclusive
                ? flatFeeAmount + adGrossRevenue * developerShare
                : adGrossRevenue * developerShare;

            return new GameEarningsDto
            {
                GameId = game.Id,
                GameTitle = game.Title,
                TotalPlays = gameSnapshots.Sum(s => s.Plays),
                CommercialBreaks = totalCommercialBreaks,
                RewardedBreaks = totalRewardedBreaks,
                FlatFeeAmount = flatFeeAmount,
                GrossEstimatedRevenue = grossRevenue,
                DeveloperEstimatedRevenue = developerRevenue,
                PlatformEstimatedRevenue = grossRevenue - developerRevenue,
                DeveloperShare = developerShare,
                ContractType = contractType,
                Daily = daily
            };
        }

        private async Task<Dictionary<Guid, List<TrafficShare>>> GetTrafficSharesAsync(List<Guid> gameIds)
        {
            var shares = await _playSessionRepository.GetAll()
                .Where(s => gameIds.Contains(s.GameId))
                .GroupBy(s => new { s.GameId, s.TrafficSource })
                .Select(g => new { g.Key.GameId, g.Key.TrafficSource, Count = g.Count() })
                .ToListAsync();

            return shares
                .GroupBy(s => s.GameId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new TrafficShare(x.TrafficSource, x.Count)).ToList());
        }

        private decimal ComputeDeveloperShare(RevenueContractType contractType, Guid gameId, Dictionary<Guid, List<TrafficShare>> trafficShares)
        {
            if (!trafficShares.TryGetValue(gameId, out var shares) || shares.Count == 0)
            {
                return RevenueSplitCalculator.GetDeveloperShare(contractType, TrafficSource.Unknown);
            }

            var total = shares.Sum(s => s.Count);
            if (total == 0)
            {
                return RevenueSplitCalculator.GetDeveloperShare(contractType, TrafficSource.Unknown);
            }

            return shares.Sum(s => s.Count * RevenueSplitCalculator.GetDeveloperShare(contractType, s.Source)) / total;
        }

        private static decimal EstimateGrossRevenue(long commercialBreaks, long rewardedBreaks)
        {
            return commercialBreaks * GameHubConsts.EstimatedCommercialBreakRevenue
                + rewardedBreaks * GameHubConsts.EstimatedRewardedBreakRevenue;
        }

        private class TrafficShare
        {
            public TrafficSource Source { get; }
            public int Count { get; }

            public TrafficShare(TrafficSource source, int count)
            {
                Source = source;
                Count = count;
            }
        }

        public async Task<AdReportDto> GetAdReportAsync(GetDeveloperEarningsInput input)
        {
            await EnsureCurrentUserIsNotSupportAsync();
            ValidateDateRange(input);

            var to = (input.To ?? Clock.Now).Date.AddDays(1).AddTicks(-1);
            var from = (input.From ?? to.AddDays(-29)).Date;

            var profile = await _developerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId.Value);
            if (profile == null)
            {
                return new AdReportDto { From = from, To = to };
            }

            var gameIds = await _gameRepository.GetAll()
                .Where(g => g.DeveloperProfileId == profile.Id && !g.IsDeleted)
                .Select(g => g.Id)
                .ToListAsync();

            var impressions = await _adImpressionRepository.GetAll()
                .Where(i => gameIds.Contains(i.GameId) && i.OccurredAt >= from && i.OccurredAt <= to)
                .ToListAsync();

            var grouped = impressions
                .GroupBy(i => new { i.GameId, i.Type, i.Provider, i.CountryCode, i.DeviceType })
                .Select(g =>
                {
                    var earnings = g.Sum(i => i.Earnings);
                    var count = g.Count();
                    return new AdReportItemDto
                    {
                        GameId = g.Key.GameId,
                        GameTitle = string.Empty,
                        Type = g.Key.Type,
                        Provider = g.Key.Provider,
                        CountryCode = g.Key.CountryCode ?? string.Empty,
                        DeviceType = g.Key.DeviceType ?? string.Empty,
                        Impressions = count,
                        Earnings = earnings,
                        Cpm = count > 0 ? earnings / count * 1000 : 0
                    };
                })
                .ToList();

            var gameTitles = await _gameRepository.GetAll()
                .Where(g => gameIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g.Title);

            foreach (var item in grouped)
            {
                item.GameTitle = gameTitles.GetValueOrDefault(item.GameId) ?? string.Empty;
            }

            return new AdReportDto
            {
                From = from,
                To = to,
                TotalImpressions = impressions.Count,
                TotalEarnings = impressions.Sum(i => i.Earnings),
                AverageCpm = impressions.Count > 0 ? impressions.Sum(i => i.Earnings) / impressions.Count * 1000 : 0,
                Items = grouped
            };
        }

        private static void ValidateDateRange(GetDeveloperEarningsInput input)
        {
            if (input.From.HasValue && input.To.HasValue && input.From.Value.Date > input.To.Value.Date)
            {
                throw new ArgumentException("The earnings period start cannot be after its end.", nameof(input));
            }
        }

        private async Task EnsureCurrentUserIsNotSupportAsync()
        {
            if (!AbpSession.UserId.HasValue)
            {
                return;
            }

            var member = await _teamMemberRepository.FirstOrDefaultAsync(m => m.UserId == AbpSession.UserId.Value);
            if (member?.Role == DeveloperTeamRole.Support)
            {
                throw new AbpAuthorizationException("Support team members cannot access earnings.");
            }
        }
    }
}
