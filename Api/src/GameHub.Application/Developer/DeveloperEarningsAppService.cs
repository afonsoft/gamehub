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

        public DeveloperEarningsAppService(
            IRepository<DeveloperProfile, Guid> developerProfileRepository,
            IRepository<Game, Guid> gameRepository,
            IRepository<GameMetricSnapshot, Guid> metricSnapshotRepository,
            IRepository<RevenueContract, Guid> revenueContractRepository,
            IRepository<PlaySession, Guid> playSessionRepository)
        {
            _developerProfileRepository = developerProfileRepository;
            _gameRepository = gameRepository;
            _metricSnapshotRepository = metricSnapshotRepository;
            _revenueContractRepository = revenueContractRepository;
            _playSessionRepository = playSessionRepository;
        }

        public async Task<DeveloperEarningsDto> GetEarningsAsync(GetDeveloperEarningsInput input)
        {
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
                .ToDictionaryAsync(c => c.GameId, c => c.ContractType);

            var trafficShares = await GetTrafficSharesAsync(gameIds);

            var gameEarnings = new List<GameEarningsDto>();
            var gameById = games.ToDictionary(g => g.Id);
            var gameSnapshots = snapshots.GroupBy(s => s.GameId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var game in games)
            {
                var gameSnapshotList = gameSnapshots.GetValueOrDefault(game.Id) ?? new List<GameMetricSnapshot>();
                var contractType = contracts.GetValueOrDefault(game.Id, RevenueContractType.NonExclusive);
                var developerShare = ComputeDeveloperShare(contractType, game.Id, trafficShares);

                var earnings = BuildGameEarnings(game, gameSnapshotList, contractType, developerShare);
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

        private GameEarningsDto BuildGameEarnings(Game game, List<GameMetricSnapshot> gameSnapshots, RevenueContractType contractType, decimal developerShare)
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
            var grossRevenue = daily.Sum(d => d.GrossEstimatedRevenue);

            return new GameEarningsDto
            {
                GameId = game.Id,
                GameTitle = game.Title,
                TotalPlays = gameSnapshots.Sum(s => s.Plays),
                CommercialBreaks = totalCommercialBreaks,
                RewardedBreaks = totalRewardedBreaks,
                GrossEstimatedRevenue = grossRevenue,
                DeveloperEstimatedRevenue = grossRevenue * developerShare,
                PlatformEstimatedRevenue = grossRevenue * (1m - developerShare),
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
    }
}
