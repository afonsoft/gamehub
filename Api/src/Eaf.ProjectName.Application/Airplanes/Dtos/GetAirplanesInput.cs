using Abp.Application.Services.Dto;

namespace Eaf.ProjectName.Airplanes.Dtos
{
    public class GetAirplanesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } = null;
    }
}