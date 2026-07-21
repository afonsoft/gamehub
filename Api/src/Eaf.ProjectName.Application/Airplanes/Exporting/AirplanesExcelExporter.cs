using Eaf.Middleware.DataExporting.Excel.EpPlus;
using Eaf.Middleware.Dto;
using Eaf.Middleware.Storage;
using Eaf.ProjectName.Airplanes.Dtos;
using System.Collections.Generic;

namespace Eaf.ProjectName.Airplanes.Exporting
{
    public class AirplanesExcelExporter : EpPlusExcelExporterBase, IAirplanesExcelExporter
    {
        public AirplanesExcelExporter(
            ITempFileCacheManager tempFileCacheManager
        ) : base(tempFileCacheManager)
        {
            LocalizationSourceName = ProjectNameConsts.LocalizationSourceName;
        }

        public FileDto ExportToFile(List<AirplaneDto> airplanes)
        {
            return CreateExcelPackage(
                "Airplanes.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Airplanes"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Number"),
                        L("Model")
                        );

                    AddObjects(
                        sheet, 2, airplanes,
                        _ => _.Number,
                        _ => _.Model
                        );
                });
        }
    }
}