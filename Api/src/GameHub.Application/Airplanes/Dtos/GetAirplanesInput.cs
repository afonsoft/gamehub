using Abp.Application.Services.Dto;

namespace GameHub.Airplanes.Dtos
{
    public class GetAirplanesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } = null;
    }
}