using AutoMapper;
using GameHub.Airplanes;
using GameHub.Airplanes.Dtos;

namespace GameHub
{
    internal static class ProjectNameCustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            /* ADD YOUR OWN CUSTOM AUTOMAPPER MAPPINGS HERE */

            configuration.CreateMap<CreateOrEditAirplaneDto, Airplane>();
            configuration.CreateMap<Airplane, AirplaneDto>();
        }
    }
}