using MediatR;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CWI.Application.Features.Import.Queries.DownloadTemplate;

public class DownloadOrderTemplateQuery : IRequest<byte[]>
{
}

public class DownloadOrderTemplateQueryHandler : IRequestHandler<DownloadOrderTemplateQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadOrderTemplateQuery request, CancellationToken cancellationToken)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("OrderTemplate");

        // Headers
        worksheet.Cells[1, 1].Value = "ProductCode";
        worksheet.Cells[1, 2].Value = "Quantity";
        worksheet.Cells[1, 3].Value = "Price";

        // Header style
        using (var range = worksheet.Cells[1, 1, 1, 3])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // Sample row
        worksheet.Cells[2, 1].Value = "PRD001";
        worksheet.Cells[2, 2].Value = 10;
        worksheet.Cells[2, 3].Value = 12.50m;

        worksheet.Column(1).AutoFit();
        worksheet.Column(2).AutoFit();
        worksheet.Column(3).AutoFit();

        return await Task.FromResult(package.GetAsByteArray());
    }
}
