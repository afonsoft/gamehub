using Abp.Application.Services.Dto;

namespace Eaf.ProjectName.Airplanes.Dtos
{
    public class AirplaneDto : EntityDto
    {
        public string Number { get; set; }
        public string Model { get; set; }
    }
}