using Microsoft.AspNetCore.Http;

namespace CWI.Application.DTOs.Orders;

public class UploadSalesOrderRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public string OrderType { get; set; } = "Order"; // Order, PreOrder, Shipped
    public IFormFile OrderFile { get; set; } = null!;
}

public class UploadSalesOrderResponse
{
    public long OrderId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
