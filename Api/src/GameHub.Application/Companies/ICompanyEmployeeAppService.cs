using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using GameHub.Companies.Dto;

namespace GameHub.Companies
{
    public interface ICompanyEmployeeAppService : IApplicationService
    {
        Task<List<CompanyEmployeeDto>> GetEmployeesAsync(int tenantId);

        Task<CompanyEmployeeDto> InviteAsync(InviteEmployeeInput input);

        Task<CompanyEmployeeDto> RegisterAndJoinAsync(JoinCompanyInput input);

        Task RemoveAsync(RemoveEmployeeInput input);

        Task SetDefaultAsync(SetDefaultEmployeeInput input);
    }
}
