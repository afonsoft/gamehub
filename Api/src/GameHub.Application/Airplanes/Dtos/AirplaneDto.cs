using Abp.Application.Services.Dto;

namespace GameHub.Airplanes.Dtos
{
    public class AirplaneDto : EntityDto
    {
        public string Number { get; set; }
        public string Model { get; set; }
    }
}