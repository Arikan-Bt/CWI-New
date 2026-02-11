using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Reports;

/// <summary>
/// Müşteri Bakiye Raporu İsteği
/// </summary>
public class CustomerBalanceReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Müşteri Bakiye Raporu DTO (Hareket bazlı)
/// </summary>
public class CustomerBalanceReportItemDto
{
    public string CurrAccCode { get; set; } = string.Empty;
    public string CurrAccDescription { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal Balance { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Müşteri Bakiye Raporu Yanıtı
/// </summary>
public class CustomerBalanceReportResponse
{
    public List<CustomerBalanceReportItemDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}
