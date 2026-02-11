using CWI.Application.DTOs.Inventory;
using MediatR;
using MiniExcelLibs;

namespace CWI.Application.Features.Inventory.Queries.GetStockAdjustmentTemplate;

public class GetStockAdjustmentTemplateQuery : IRequest<byte[]>
{
}

public class GetStockAdjustmentTemplateHandler : IRequestHandler<GetStockAdjustmentTemplateQuery, byte[]>
{
    public Task<byte[]> Handle(GetStockAdjustmentTemplateQuery request, CancellationToken cancellationToken)
    {
        // Örnek veri (boş şablon veya örnek satırlı)
        var templateData = new List<StockAdjustmentExcelModel>
        {
            new() { 
                ProductCode = "SKU123", 
                Quantity = 100, 
                Supplier = "Example Supplier", 
                Price = 10.5m, 
                Currency = "USD", 
                Warehouse = "Main Warehouse", 
                ShelfNumber = "A-01-01", 
                PackList = "PL-2023-001", 
                ReceivingNo = "REC-001" 
            }
        };

        using var memoryStream = new MemoryStream();
        memoryStream.SaveAs(templateData);
        return Task.FromResult(memoryStream.ToArray());
    }
}
