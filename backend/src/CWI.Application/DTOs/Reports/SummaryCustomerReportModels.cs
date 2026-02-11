using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Reports;

/// <summary>
/// Müşteri Özet Raporu İsteği
/// </summary>
public class SummaryCustomerReportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Müşteri Özet Raporu Öğe DTO
/// </summary>
public class SummaryCustomerItemDto
{
    public string AccountDescription { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RecBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Müşteri Özet Raporu Yanıtı
/// </summary>
public class SummaryCustomerReportResponse
{
    public List<SummaryCustomerItemDto> Data { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalRecBalance { get; set; }
    public int TotalCount { get; set; }
}
