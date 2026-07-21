using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Admin.Dto;

namespace GameHub.Admin
{
    /// <summary>
    /// Contrato de serviço de auditoria.
    /// </summary>
    public interface IAuditLogAppService : IApplicationService
    {
        Task<PagedResultDto<AuditLogDto>> GetAllAsync(GetAuditLogsInput input);
    }
}
