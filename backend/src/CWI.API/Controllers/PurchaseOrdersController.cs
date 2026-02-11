using CWI.Application.Common.Models;
using CWI.Application.DTOs.Purchasing;
using CWI.Application.Features.Purchasing.Commands.CreatePurchaseOrderFromExcel;
using CWI.Application.Features.Purchasing.Commands.SavePurchaseOrderInvoice;
using CWI.Application.Features.Purchasing.Commands.UpdatePurchaseOrderStatus;
using CWI.Application.Features.Purchasing.Queries.GetPurchaseOrderDetails;
using CWI.Application.Features.Purchasing.Queries.GetPurchaseOrders;
using CWI.Application.Features.Purchasing.Queries.GetPurchaseOrderTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Satın alma siparişlerini listeler
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>Sipariş listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PurchaseOrderListResponse>), 200)]
    public async Task<IActionResult> GetOrders([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPurchaseOrdersQuery { StartDate = startDate, EndDate = endDate, Page = page, PageSize = pageSize });
        return Ok(Result<PurchaseOrderListResponse>.Succeed(result));
    }

    /// <summary>
    /// Belirli bir satın alma siparişinin detaylarını getirir
    /// </summary>
    /// <param name="id">Sipariş Id</param>
    /// <returns>Sipariş detayları</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<PurchaseOrderDetailDto>), 200)]
    public async Task<IActionResult> GetOrderDetails(long id)
    {
        var result = await _mediator.Send(new GetPurchaseOrderDetailsQuery { Id = id });
        
        if (result == null || result.Id == 0)
        {
            return NotFound(Result<PurchaseOrderDetailDto>.Failure("Order details not found."));
        }

        return Ok(Result<PurchaseOrderDetailDto>.Succeed(result));
    }

    /// <summary>
    /// Satın alma siparişi durumunu günceller
    /// </summary>
    /// <param name="id">Sipariş Id</param>
    /// <param name="command">Durum güncelleme komutu</param>
    /// <returns>Başarı durumu</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdatePurchaseOrderStatusCommand command)
    {
        if (id != command.Id) return BadRequest(Result.Failure("ID mismatch."));
        var result = await _mediator.Send(command);
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Satın alma siparişi için fatura kaydeder (teslim alınan miktarları günceller)
    /// </summary>
    /// <param name="id">Sipariş Id</param>
    /// <param name="command">Fatura kayıt komutu</param>
    /// <returns>Başarı durumu</returns>
    [HttpPost("{id}/invoice")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> SaveInvoice(long id, [FromForm] SavePurchaseOrderInvoiceCommand command)
    {
        if (id != command.OrderId) return BadRequest(Result.Failure("ID mismatch."));
        var result = await _mediator.Send(command);
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Excel dosyası ile toplu satın alma siparişi girişi yapar.
    /// </summary>
    /// <param name="request">Sipariş bilgileri ve Excel dosyası</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("upload-excel")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<CreatePurchaseOrderFromExcelResponse>), 200)]
    public async Task<ActionResult<Result<CreatePurchaseOrderFromExcelResponse>>> UploadExcel([FromForm] CreatePurchaseOrderFromExcelRequest request)
    {
        var result = await _mediator.Send(new CreatePurchaseOrderFromExcelCommand(request));
        if (result.Id == 0)
        {
            return BadRequest(Result<CreatePurchaseOrderFromExcelResponse>.Failure(result.Message ?? "Import failed."));
        }
        return Ok(Result<CreatePurchaseOrderFromExcelResponse>.Succeed(result));
    }

    /// <summary>
    /// Satın alma siparişi için Excel şablonu indirir.
    /// </summary>
    /// <returns>Excel şablon dosyası</returns>
    [HttpGet("template")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public async Task<IActionResult> DownloadTemplate()
    {
        var fileContent = await _mediator.Send(new GetPurchaseOrderTemplateQuery());
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PurchaseOrderTemplate.xlsx");
    }
}
