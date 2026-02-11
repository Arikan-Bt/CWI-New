using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs.Attributes;

namespace CWI.Application.DTOs.Purchasing;

/// <summary>
/// Excel'den satın alma siparişi oluşturma isteği
/// </summary>
public class CreatePurchaseOrderFromExcelRequest
{
    [FromForm(Name = "vendorCode")]
    public string VendorCode { get; set; } = string.Empty;

    [FromForm(Name = "orderDate")]
    public DateTime OrderDate { get; set; }

    [FromForm(Name = "deliveryDate")]
    public DateTime? DeliveryDate { get; set; }

    [FromForm(Name = "description")]
    public string Description { get; set; } = string.Empty;

    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// Satın alma siparişi oluşturma yanıtı
/// </summary>
public class CreatePurchaseOrderFromExcelResponse
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ProcessedItemsCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Excel'den okunan sipariş satırı modeli
/// </summary>
public class PurchaseOrderExcelModel
{
    [ExcelColumnName("Product Code", Aliases = new[] { "ProductCode", "Product Code", "SKU", "UrunKodu" })]
    public string ProductCode { get; set; } = string.Empty;
    
    [ExcelColumnName("Quantity", Aliases = new[] { "Quantity", "Qty", "Adet", "Miktar" })]
    public int Quantity { get; set; }
    
    [ExcelColumnName("Unit Price", Aliases = new[] { "Unit Price", "UnitPrice", "Maliyet", "BirimFiyat" })]
    public decimal UnitPrice { get; set; }
}
