using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;
using GameHub.Moderation.Dto;

namespace GameHub.Moderation
{
    /// <summary>
    /// Contrato de serviço de reports de usuários.
    /// </summary>
    public interface IUserReportAppService : IApplicationService
    {
        Task<UserReportDto> SubmitAsync(UserReportInput input);

        Task<PagedResultDto<UserReportDto>> GetAllAsync(GetReportsInput input);
    }
}
