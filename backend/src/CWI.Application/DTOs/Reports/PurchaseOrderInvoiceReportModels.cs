using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Reports;

/// <summary>
/// Satın Alma Siparişi Fatura Raporu isteği
/// </summary>
public class PurchaseOrderInvoiceReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchQuery { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Satın Alma Siparişi Fatura Raporu DTO
/// </summary>
public class PurchaseOrderInvoiceReportDto
{
    public long Id { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string InvoiceRefNum { get; set; } = string.Empty;
    public string OrderRefNo { get; set; } = string.Empty;
    public int InvoiceQty { get; set; }
    public decimal InvoiceAmount { get; set; }
    public int OrderQty { get; set; }
    public decimal OrderAmount { get; set; }
    public int PendingQty => OrderQty - InvoiceQty;
    public decimal PendingAmount => OrderAmount - InvoiceAmount;
}

/// <summary>
/// Satın Alma Siparişi Fatura Raporu yanıtı
/// </summary>
public class PurchaseOrderInvoiceReportResponse
{
    public List<PurchaseOrderInvoiceReportDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}
