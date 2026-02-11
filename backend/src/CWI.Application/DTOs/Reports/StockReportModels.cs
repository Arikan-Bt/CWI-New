namespace CWI.Application.DTOs.Reports;

public class StockReportRequest
{
    public string? SearchValue { get; set; }
    public string? Brand { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Sorting
    public string? SortField { get; set; }
    public int SortOrder { get; set; } // 1: Asc, -1: Desc

    // Column Filters
    public string? FilterItemCode { get; set; }
    public string? FilterItemDescription { get; set; }
    public string? FilterShelfNumber { get; set; }
}

public class StockReportResponse
{
    public List<StockReportItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class StockReportItemDto
{
    public int ProductId { get; set; }
    public string? Picture { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int Reserved { get; set; }
    public int Available { get; set; }
    public int IncomingStock { get; set; }
    public decimal RetailSalesPrice { get; set; }
    public string? SpecialNote { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? ShelfNumber { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Color { get; set; }
    public string? Attributes { get; set; }
    public List<StockReportDetailDto> Details { get; set; } = new();
}

public class StockReportDetailDto
{
    public string? ShelfNumber { get; set; }
    public string? PackList { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public DateTime? OccurredAt { get; set; }
    public int Quantity { get; set; }
    public string? MovementType { get; set; }
    public string? MovementGroup { get; set; }
    public string? ReferenceNo { get; set; }
    public string? SourceDocumentType { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? SupplierName { get; set; }
}

public class UpdateStockNoteRequest
{
    public string ItemCode { get; set; } = string.Empty;
    public string? Note { get; set; }
}
