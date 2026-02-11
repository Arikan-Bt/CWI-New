using CWI.Application.DTOs.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CWI.Application.Features.Orders.Commands.UploadSalesOrder;

public class UploadSalesOrderCommand : IRequest<UploadSalesOrderResponse>
{
    public string CustomerCode { get; set; } = string.Empty;
    public string OrderType { get; set; } = "Order";
    public IFormFile OrderFile { get; set; } = null!;
}
