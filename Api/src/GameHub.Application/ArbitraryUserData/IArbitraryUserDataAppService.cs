using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.ArbitraryUserData.Dto;

namespace GameHub.ArbitraryUserData
{
    /// <summary>
    /// Application service for arbitrary key/value JSON storage per game/user.
    /// </summary>
    public interface IArbitraryUserDataAppService : IApplicationService
    {
        Task<string> GetAsync(GetArbitraryUserDataInput input);

        Task<ArbitraryUserDataSaveResultDto> SetAsync(SetArbitraryUserDataInput input);

        Task DeleteAsync(DeleteArbitraryUserDataInput input);

        Task<ArbitraryUserDataQuotaDto> GetQuotaAsync(Guid gameId);
    }
}
