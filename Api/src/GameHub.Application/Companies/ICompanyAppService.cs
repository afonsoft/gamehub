using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using GameHub.Companies.Dto;

namespace GameHub.Companies
{
    public interface ICompanyAppService : IApplicationService
    {
        Task<PagedResultDto<CompanyDto>> GetAllAsync(PagedAndSortedResultRequestDto input);

        Task<CompanyDto> GetAsync(int id);

        Task<CompanyDto> GetByTenancyNameAsync(string tenancyName);

        Task<CompanyDto> CreateAsync(CreateOrUpdateCompanyInput input);

        Task<CompanyDto> UpdateAsync(int id, CreateOrUpdateCompanyInput input);

        Task DeleteAsync(int id);
    }
}
