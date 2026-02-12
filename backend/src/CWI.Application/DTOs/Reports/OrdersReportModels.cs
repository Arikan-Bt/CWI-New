using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Reports;

/// <summary>
/// Sipariş raporu talep modeli
/// </summary>
public class OrdersReportRequest
{
    public string? CurrentAccountCode { get; set; }
    public string? OrderStatus { get; set; }
    public bool DisplayProductPhoto { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public int SortOrder { get; set; }
    public string? Brand { get; set; }
}

/// <summary>
/// Sipariş raporu kalem modeli
/// </summary>
public class OrderReportDto
{
    public long OrderId { get; set; }
    public string CurrentAccountCode { get; set; } = string.Empty;
    public string CurrentAccountDescription { get; set; } = string.Empty;
    public string OrderDetails { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequestedShipmentDate { get; set; }
    public decimal TotalQty { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    // Edit paneli için eklenen alanlar
    public string? Address { get; set; }
    public string? PaymentType { get; set; }
    public string? ShipmentMethod { get; set; }
    public string? OrderDescription { get; set; }
    public decimal SubTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Season { get; set; }
}

/// <summary>
/// Sipariş güncelleme isteği
/// </summary>
public class UpdateOrderRequest
{
    public long OrderId { get; set; }
    public string? PaymentType { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime? RequestedShipmentDate { get; set; }
    public string? ShipmentMethod { get; set; }
    public string? Status { get; set; }
    public string? OrderDescription { get; set; }
    public List<string>? RemovedProductCodes { get; set; }
    public List<OrderWarehouseSelectionDto>? WarehouseSelections { get; set; }
}

public class OrderWarehouseSelectionDto
{
    public string ProductCode { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
}

public class UpdateOrderItemRequest
{
    public long OrderId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class RemoveOrderItemRequest
{
    public long OrderId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
}

/// <summary>
/// Sipariş detay kalem modeli
/// </summary>
public class OrderDetailDto
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Picture { get; set; }
    public int Qty { get; set; }
    public decimal Amount { get; set; }
    public decimal Total { get; set; }
    public string? Attributes { get; set; }
}

/// <summary>
/// Sipariş detay yanıt modeli
/// </summary>
public class OrderDetailResponse
{
    public List<OrderDetailDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// Sipariş raporu yanıt modeli
/// </summary>
public class OrdersReportResponse
{
    public List<string> Brands { get; set; } = new();
    public List<OrderReportDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}
