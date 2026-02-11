using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Reports;

/// <summary>
/// Müşteri Ödeme Detay Raporu İsteği
/// </summary>
public class CustomerPaymentDetailReportRequest
{
    public string? CustomerCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Müşteri Ödeme Detay Raporu DTO
/// </summary>
public class CustomerPaymentDetailItemDto
{
    public DateTime Date { get; set; }
    public string RefNo1 { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public string RefNo2 { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string? ReceiptFilePath { get; set; }
}

/// <summary>
/// Müşteri Ödeme Detay Raporu Yanıtı
/// </summary>
public class CustomerPaymentDetailReportResponse
{
    public List<CustomerPaymentDetailItemDto> Data { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalBalance { get; set; }
    public int TotalCount { get; set; }
}
