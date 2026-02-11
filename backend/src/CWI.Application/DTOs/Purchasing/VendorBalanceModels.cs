namespace CWI.Application.DTOs.Purchasing;

public class VendorBalanceReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Sort
    public string? SortField { get; set; }
    public int SortOrder { get; set; }

    // Filters
    public string? FilterCurrAccCode { get; set; }
    public string? FilterCurrAccDescription { get; set; }
    public string? FilterInvoiceNo { get; set; }
    public string? FilterDescription { get; set; }
}

public class VendorBalanceReportItemDto
{
    public string CurrAccCode { get; set; } = string.Empty;
    public string CurrAccDescription { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaymentTotal { get; set; }
    public decimal Balance { get; set; }
    public string? InvoiceFilePath { get; set; }
    public string? PaymentFilePath { get; set; }
    public List<PaymentHistoryItemDto> PaymentHistory { get; set; } = new();
}

public class PaymentHistoryItemDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class VendorBalanceReportResponse
{
    public List<VendorBalanceReportItemDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}
