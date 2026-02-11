using CWI.Application.DTOs.Orders;
using CWI.Application.Features.Orders.Commands.UploadSalesOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Satış siparişi dosyasını yükler ve sipariş oluşturur.
    /// </summary>
    [HttpPost("upload-sales-order")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadSalesOrderResponse>> UploadSalesOrder([FromForm] UploadSalesOrderCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
