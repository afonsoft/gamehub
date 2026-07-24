using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Monetization.Dto;

namespace GameHub.Monetization
{
    /// <summary>
    /// Service for managing revenue contracts and calculating revenue splits.
    /// </summary>
    public interface IRevenueContractAppService : IApplicationService
    {
        Task<RevenueContractDto> GetByGameAsync(Guid gameId);

        Task<RevenueContractDto> SetContractAsync(Guid gameId, RevenueContractType contractType);

        Task<RevenueShareResultDto> CalculateShareAsync(Guid gameId, TrafficSource trafficSource);
    }
}
