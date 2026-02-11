using System.Text.Json.Serialization;

namespace CWI.Application.DTOs.Purchasing;

public class PurchaseOrderDetailDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("orderRefNo")]
    public string OrderRefNo { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("items")]
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("productCode")]
    public string ProductCode { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("orderQty")]
    public int OrderQty { get; set; }

    [JsonPropertyName("orderUnitPrice")]
    public decimal OrderUnitPrice { get; set; }

    [JsonPropertyName("orderAmount")]
    public decimal OrderAmount { get; set; }

    [JsonPropertyName("receive")]
    public int Receive { get; set; }

    [JsonPropertyName("balance")]
    public int Balance { get; set; }

    [JsonPropertyName("invoiceQty")]
    public int InvoiceQty { get; set; }

    [JsonPropertyName("invoiceUnitPrice")]
    public decimal InvoiceUnitPrice { get; set; }
}
