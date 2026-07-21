using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Eaf.Middleware.Dto;
using GameHub.Airplanes.Dtos;
using GameHub.Airplanes.Exporting;
using GameHub.Airplanes.Jobs;
using GameHub.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace GameHub.Airplanes
{
    [AbpAuthorize(ProjectNamePermissions.Pages_Airplanes)]
    public class AirplanesAppService : ProjectNameAppServiceBase, IAirplanesAppService
    {
        private readonly IAirplaneJob _airplaneJob;
        private readonly IAirplaneManager _airplaneManager;
        private readonly IAirplanesExcelExporter _airplanesExcelExporter;

        public AirplanesAppService(
            IAirplaneJob airplaneJob,
            IAirplaneManager airplaneManager,
            IAirplanesExcelExporter airplanesExcelExporter
        )
        {
            LocalizationSourceName = ProjectNameConsts.LocalizationSourceName;

            _airplaneJob = airplaneJob;
            _airplaneManager = airplaneManager;
            _airplanesExcelExporter = airplanesExcelExporter;
        }

        public async Task<PagedResultDto<AirplaneDto>> GetAll(GetAirplanesInput input)
        {
            var query = _airplaneManager.Airplanes
                .AsNoTracking()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    e => e.Number.Contains(input.Filter) || e.Model.Contains(input.Filter));

            var total = await query.CountAsync();
            var items = await query.OrderBy(input.Sorting ?? "id asc").PageBy(input).ToListAsync();

            return new PagedResultDto<AirplaneDto>(total, ObjectMapper.Map<List<AirplaneDto>>(items));
        }

        [AbpAuthorize(ProjectNamePermissions.Pages_Airplanes_Edit)]
        public async Task<CreateOrEditAirplaneDto> GetAirplaneForEdit(EntityDto input)
        {
            var airplane = await _airplaneManager.GetByIdAsync(input.Id);
            return ObjectMapper.Map<CreateOrEditAirplaneDto>(airplane);
        }

        public async Task CreateOrEdit(CreateOrEditAirplaneDto input)
        {
            if (input.Id.HasValue)
                await Update(input);
            else
                await Create(input);
        }

        [AbpAuthorize(ProjectNamePermissions.Pages_Airplanes_Create)]
        private async Task Create(CreateOrEditAirplaneDto input)
        {
            var airplane = ObjectMapper.Map<Airplane>(input);

            if (AbpSession.TenantId != null)
                airplane.TenantId = AbpSession.TenantId;

            await _airplaneManager.CreateAsync(airplane);
        }

        [AbpAuthorize(ProjectNamePermissions.Pages_Airplanes_Edit)]
        private async Task Update(CreateOrEditAirplaneDto input)
        {
            var airplane = await _airplaneManager.GetByIdAsync(input.Id.Value);
            ObjectMapper.Map(input, airplane);
            await _airplaneManager.UpdateAsync(airplane);
        }

        [AbpAuthorize(ProjectNamePermissions.Pages_Airplanes_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _airplaneManager.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetAirplanesToExcel()
        {
            var items = await _airplaneManager.Airplanes.AsNoTracking().ToListAsync();
            return _airplanesExcelExporter.ExportToFile(ObjectMapper.Map<List<AirplaneDto>>(items));
        }

        public Task StartJob()
        {
            return _airplaneJob.StartProcess();
        }
    }
}