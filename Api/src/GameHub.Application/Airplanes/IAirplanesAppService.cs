using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Eaf.Middleware.Dto;
using GameHub.Airplanes.Dtos;
using System.Threading.Tasks;

namespace GameHub.Airplanes
{
    public interface IAirplanesAppService : IApplicationService
    {
        Task<PagedResultDto<AirplaneDto>> GetAll(GetAirplanesInput input);

        Task<CreateOrEditAirplaneDto> GetAirplaneForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditAirplaneDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetAirplanesToExcel();

        Task StartJob();
    }
}