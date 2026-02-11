using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;

namespace CWI.Application.Features.Import.Queries.ValidateOrder;

public class ValidateOrderQuery : IRequest<ValidateOrderResponse>
{
    public string FileContent { get; set; } = string.Empty;
}

public class ValidateOrderResponse
{
    public bool IsValid { get; set; }
    public int TotalRows { get; set; }
    public int ErrorCount { get; set; }
    public List<ValidationErrorDto> Details { get; set; } = new();
}

public class ValidationErrorDto
{
    public int Row { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ValidateOrderQueryHandler : IRequestHandler<ValidateOrderQuery, ValidateOrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ValidateOrderQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidateOrderResponse> Handle(ValidateOrderQuery request, CancellationToken cancellationToken)
    {
        var response = new ValidateOrderResponse();

        try
        {
            var bytes = Convert.FromBase64String(request.FileContent.Contains(",")
                ? request.FileContent.Split(',')[1]
                : request.FileContent);

            using var stream = new MemoryStream(bytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            response.TotalRows = rowCount - 1;

            for (int row = 2; row <= rowCount; row++)
            {
                var productCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var quantityStr = worksheet.Cells[row, 2].Value?.ToString();
                var priceStr = worksheet.Cells[row, 3].Value?.ToString();

                if (string.IsNullOrWhiteSpace(productCode))
                {
                    response.Details.Add(new ValidationErrorDto { Row = row, Message = "Product code is required." });
                    continue;
                }

                var productExists = await _unitOfWork.Repository<Product, int>()
                    .AsQueryable()
                    .AnyAsync(p => p.Sku == productCode, cancellationToken);

                if (!productExists)
                {
                    response.Details.Add(new ValidationErrorDto { Row = row, Message = $"Product not found: {productCode}" });
                }

                if (!int.TryParse(quantityStr, out var qty) || qty <= 0)
                {
                    response.Details.Add(new ValidationErrorDto { Row = row, Message = "Quantity must be a positive number." });
                }

                var parsedPrice = 0m;
                if (!string.IsNullOrWhiteSpace(priceStr) && !TryParsePrice(priceStr, out parsedPrice))
                {
                    response.Details.Add(new ValidationErrorDto { Row = row, Message = "Price is not a valid number." });
                }
                else if (!string.IsNullOrWhiteSpace(priceStr) && parsedPrice < 0)
                {
                    response.Details.Add(new ValidationErrorDto { Row = row, Message = "Price cannot be negative." });
                }
            }

            response.ErrorCount = response.Details.Count;
            response.IsValid = response.ErrorCount == 0;
        }
        catch (Exception ex)
        {
            response.IsValid = false;
            response.Details.Add(new ValidationErrorDto { Row = 0, Message = "File read error: " + ex.Message });
            response.ErrorCount = response.Details.Count;
        }

        return response;
    }

    private static bool TryParsePrice(string raw, out decimal value)
    {
        var normalized = raw.Trim().Replace(" ", string.Empty);

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out value))
        {
            return true;
        }

        normalized = normalized.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
