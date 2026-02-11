using MediatR;
using MiniExcelLibs;
using CWI.Application.DTOs.Purchasing;

namespace CWI.Application.Features.Purchasing.Queries.GetPurchaseOrderTemplate;

/// <summary>
/// Satın alma siparişi Excel şablonu indirme sorgusu
/// </summary>
public class GetPurchaseOrderTemplateQuery : IRequest<byte[]>
{
}

public class GetPurchaseOrderTemplateHandler : IRequestHandler<GetPurchaseOrderTemplateQuery, byte[]>
{
    public Task<byte[]> Handle(GetPurchaseOrderTemplateQuery request, CancellationToken cancellationToken)
    {
        // Örnek şablon verisi
        var templateData = new List<PurchaseOrderExcelModel>
        {
            new() { 
                ProductCode = "SKU123", 
                Quantity = 10, 
                UnitPrice = 100.00m
            }
        };

        using var memoryStream = new MemoryStream();
        memoryStream.SaveAs(templateData);
        return Task.FromResult(memoryStream.ToArray());
    }
}
