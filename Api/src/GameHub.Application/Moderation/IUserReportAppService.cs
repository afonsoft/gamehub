using System.Threading.Tasks;
using Abp.Application.Services;
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
    }
}
