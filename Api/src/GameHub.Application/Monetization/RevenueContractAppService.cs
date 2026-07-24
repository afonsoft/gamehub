using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using GameHub.Authorization;
using GameHub.Catalog;
using GameHub.Monetization.Dto;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Monetization
{
    /// <summary>
    /// Manages revenue contracts and calculates revenue splits based on traffic source.
    /// </summary>
    [AbpAuthorize(GameHubPermissions.Pages_Developer_Games)]
    public class RevenueContractAppService : GameHubAppServiceBase, IRevenueContractAppService
    {
        private readonly IRepository<RevenueContract, Guid> _contractRepository;
        private readonly IGameCatalogCache _catalogCache;

        public RevenueContractAppService(IRepository<RevenueContract, Guid> contractRepository, IGameCatalogCache catalogCache)
        {
            _contractRepository = contractRepository;
            _catalogCache = catalogCache;
        }

        public async Task<RevenueContractDto> GetByGameAsync(Guid gameId)
        {
            var contract = await _contractRepository.GetAll()
                .Where(c => c.GameId == gameId && c.IsActive)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync();

            if (contract == null)
            {
                return null;
            }

            return Map(contract);
        }

        public async Task<RevenueContractDto> SetContractAsync(Guid gameId, RevenueContractType contractType)
        {
            var existing = await _contractRepository.GetAll()
                .Where(c => c.GameId == gameId && c.IsActive)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync();

            if (existing != null && existing.ContractType == contractType)
            {
                return Map(existing);
            }

            if (existing != null)
            {
                existing.IsActive = false;
            }

            var contract = new RevenueContract(Guid.NewGuid(), gameId, contractType)
            {
                TenantId = AbpSession.TenantId
            };
            await _contractRepository.InsertAsync(contract);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _catalogCache.InvalidateHomeAsync();
            await _catalogCache.InvalidateSearchAsync();

            return Map(contract);
        }

        public async Task<RevenueShareResultDto> CalculateShareAsync(Guid gameId, TrafficSource trafficSource)
        {
            var contract = await _contractRepository.GetAll()
                .Where(c => c.GameId == gameId && c.IsActive)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync();

            var contractType = contract?.ContractType ?? RevenueContractType.NonExclusive;

            return new RevenueShareResultDto
            {
                TrafficSource = trafficSource,
                ContractType = contractType,
                DeveloperShare = RevenueSplitCalculator.GetDeveloperShare(contractType, trafficSource),
                PlatformShare = RevenueSplitCalculator.GetPlatformShare(contractType, trafficSource)
            };
        }

        private static RevenueContractDto Map(RevenueContract contract)
        {
            return new RevenueContractDto
            {
                Id = contract.Id,
                GameId = contract.GameId,
                ContractType = contract.ContractType,
                EffectiveDate = contract.EffectiveDate,
                IsActive = contract.IsActive
            };
        }
    }
}
