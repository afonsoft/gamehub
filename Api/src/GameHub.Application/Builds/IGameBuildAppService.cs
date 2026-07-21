using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Developer.Dto;

namespace GameHub.Builds
{
    public interface IGameBuildAppService : IApplicationService
    {
        Task<UploadGameBuildResultDto> UploadBuildAsync(Guid gameId, Stream packageStream, string fileName, string contentType);
    }
}
