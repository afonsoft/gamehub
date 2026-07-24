using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Inspector.Dto;

namespace GameHub.Inspector
{
    public interface IInspectorAppService : IApplicationService
    {
        Task<InspectorSessionDto> StartSessionAsync(StartInspectorSessionInput input);
        Task LogSdkEventAsync(LogSdkEventInput input);
        Task AddWarningAsync(AddInspectorWarningInput input);
        Task<InspectorSessionDetailDto> GetSessionAsync(Guid sessionId);
        Task<List<InspectorSessionDto>> GetSessionsAsync(Guid gameId, int maxResultCount = 20);
        Task<List<InspectorWarningDto>> ValidateSessionAsync(Guid sessionId);
        Task SaveChecklistAnswerAsync(SaveChecklistAnswerInput input);
        Task<InspectorChecklistCompletionDto> GetChecklistCompletionAsync(Guid sessionId);
    }
}
