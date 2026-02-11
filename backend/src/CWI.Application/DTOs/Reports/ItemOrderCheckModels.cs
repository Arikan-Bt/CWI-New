using System;
using System.Collections.Generic;

namespace CWI.Application.DTOs.Reports;

public class ItemOrderCheckRequest
{
    public string Sku { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ItemOrderCheckResponse
{
    public ItemOrderSummaryDto Summary { get; set; } = new();
    public List<ItemMovementDto> Movements { get; set; } = new();
    public int TotalMovements { get; set; }
}

public class ItemOrderSummaryDto
{
    public string ItemCode { get; set; } = string.Empty;
    public decimal PurchaseQty { get; set; }
    public decimal SalesQty { get; set; }
    public decimal WareHouseQty { get; set; }
    public decimal ReserveQty { get; set; }
    public decimal AvailableQty { get; set; }
    public decimal IncomingStock { get; set; }
}

public class ItemMovementDto
{
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string ItemCode { get; set; } = string.Empty;
}
