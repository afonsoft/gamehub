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

        Task<PlaytestRecordingDto> GetRecordingAsync(Guid recordingId);

        Task<ListResultDto<PlaytestRecordingDto>> ListRecordingsAsync(Guid playtestId);

        Task<PagedResultDto<PlaytestRecordingDto>> GetAllRecordingsAsync(GetAllPlaytestRecordingsInput input);

        Task<PlaytestRecordingDto> AddNotesAsync(AddPlaytestRecordingNotesInput input);

        Task<PlaytestDifficultyInsightDto> GetDifficultyInsightsAsync(Guid gameId);
    }
}
