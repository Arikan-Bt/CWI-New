using Microsoft.AspNetCore.Http;

namespace CWI.Application.DTOs.Inventory;

/// <summary>
/// Stok düzenleme isteği DTO
/// </summary>
public record CreateStockAdjustmentRequest
{
    public DateTime AdjustmentDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public IFormFile File { get; init; } = null!;
}

/// <summary>
/// Stok düzenleme yanıt DTO
/// </summary>
public record CreateStockAdjustmentResponse
{
    public long Id { get; init; }
    public int ProcessedItemsCount { get; init; }
    public int SkippedItemsCount { get; init; }
    public List<StockAdjustmentWarningDto> Warnings { get; init; } = new();
    public string Message { get; init; } = string.Empty;
}

public record StockAdjustmentWarningDto
{
    public int Row { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Excel'den okunan satır modeli
/// </summary>
public class StockAdjustmentExcelModel
{
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Supplier { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? Warehouse { get; set; }
    public string? ShelfNumber { get; set; }
    public string? PackList { get; set; }
    public string? ReceivingNo { get; set; }
}
