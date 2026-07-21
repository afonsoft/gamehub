using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Airplanes.Dtos
{
    [AutoMap(typeof(Airplane))]
    public class CreateOrEditAirplaneDto : EntityDto<int?>
    {
        [Required]
        public string Number { get; set; }

        [Required]
        [StringLength(Airplane.MaxModelLength)]
        public string Model { get; set; }
    }
}