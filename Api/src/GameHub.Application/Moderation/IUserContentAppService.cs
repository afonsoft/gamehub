using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Moderation.Dto;

namespace GameHub.Moderation
{
    public interface IUserContentAppService : IApplicationService
    {
        Task<UserContentDto> SubmitAsync(SubmitUserContentInput input);
        Task<List<UserContentDto>> GetPendingAsync(int maxResultCount = 50);
        Task<UserContentDto> ModerateAsync(ModerateUserContentInput input);
        Task<List<UserContentDto>> GetByGameAsync(Guid gameId, bool onlyApproved = true);
    }
}
