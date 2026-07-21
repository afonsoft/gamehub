using AutoMapper;
using Eaf.ProjectName.Airplanes;
using Eaf.ProjectName.Airplanes.Dtos;

namespace Eaf.ProjectName
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