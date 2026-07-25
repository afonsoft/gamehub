using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Playtesting.Dto;

namespace GameHub.Playtesting
{
    public interface IPlaytestAppService : IApplicationService
    {
        Task<PlaytestSessionDto> RequestPlaytestAsync(RequestPlaytestInput input);

        Task<ListResultDto<PlaytestSessionDto>> GetPlaytestsByGameAsync(Guid gameId);

        Task<PlaytestSessionDto> UploadRecordingAsync(UploadPlaytestRecordingInput input);
    }
}
