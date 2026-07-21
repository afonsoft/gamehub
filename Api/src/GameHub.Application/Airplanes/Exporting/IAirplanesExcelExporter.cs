using Eaf.Middleware.Dto;
using GameHub.Airplanes.Dtos;
using System.Collections.Generic;

namespace GameHub.Airplanes.Exporting
{
    public interface IAirplanesExcelExporter
    {
        FileDto ExportToFile(List<AirplaneDto> airplanes);
    }
}