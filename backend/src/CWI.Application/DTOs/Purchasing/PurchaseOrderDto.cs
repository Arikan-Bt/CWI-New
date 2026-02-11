using System.Text.Json.Serialization;

namespace CWI.Application.DTOs.Purchasing;

public class PurchaseOrderDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("orderRefNo")]
    public string OrderRefNo { get; set; } = string.Empty;

    [JsonPropertyName("documentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonPropertyName("customerSvc")]
    public string CustomerSvc { get; set; } = string.Empty;

    [JsonPropertyName("qty")]
    public int Qty { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class PurchaseOrderListResponse
{
    [JsonPropertyName("data")]
    public List<PurchaseOrderDto> Data { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}
