using Eaf.Middleware.Dto;
using Eaf.ProjectName.Airplanes.Dtos;
using System.Collections.Generic;

namespace Eaf.ProjectName.Airplanes.Exporting
{
    public interface IAirplanesExcelExporter
    {
        FileDto ExportToFile(List<AirplaneDto> airplanes);
    }
}