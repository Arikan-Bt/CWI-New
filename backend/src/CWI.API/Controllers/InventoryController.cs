using CWI.Application.Common.Models;
using CWI.Application.DTOs.Common;
using CWI.Application.DTOs.Inventory;
using CWI.Application.Features.Inventory.Commands.CreateStockAdjustment;
using CWI.Application.Features.Inventory.Commands.CreateWarehouse;
using CWI.Application.Features.Inventory.Commands.DeleteWarehouse;
using CWI.Application.Features.Inventory.Commands.UpdateWarehouse;
using CWI.Application.Features.Inventory.Queries.CheckOrderStock;
using CWI.Application.Features.Inventory.Queries.GetWarehouseById;
using CWI.Application.Features.Inventory.Queries.GetWarehouses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Excel dosyası ile stok düzenlemesi yapar.
    /// </summary>
    /// <param name="request">Düzenleme bilgileri ve Excel dosyası</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPost("stock-adjustment")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<CreateStockAdjustmentResponse>>> CreateStockAdjustment([FromForm] CreateStockAdjustmentRequest request)
    {
        var command = new CreateStockAdjustmentCommand(request);
        var result = await _mediator.Send(command);
        
        if (result.Id == 0)
        {
            return BadRequest(Result<CreateStockAdjustmentResponse>.Failure("Düzenleme kaydı oluşturulamadı."));
        }

        return Ok(Result<CreateStockAdjustmentResponse>.Succeed(result));
    }

    /// <summary>
    /// Aktif depoları listeler (dropdown için)
    /// </summary>
    /// <returns>Depo listesi</returns>
    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(Result<List<WarehouseDto>>), 200)]
    public async Task<IActionResult> GetWarehouses([FromQuery] bool bypassCache = false)
    {
        var result = await _mediator.Send(new GetWarehousesQuery { BypassCache = bypassCache });
        return Ok(Result<List<WarehouseDto>>.Succeed(result));
    }

    /// <summary>
    /// Depoları paginated listeler (yönetim paneli için)
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <param name="searchTerm">Arama terimi</param>
    /// <returns>Paginated depo listesi</returns>
    [HttpGet("warehouses/paginated")]
    [ProducesResponseType(typeof(Result<PagedResult<WarehouseDto>>), 200)]
    public async Task<IActionResult> GetWarehousesPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int sortOrder = 1,
        [FromQuery] string? filterCode = null,
        [FromQuery] string? filterName = null,
        [FromQuery] string? filterAddress = null,
        [FromQuery] string? filterStatus = null,
        [FromQuery] string? filterDefault = null)
    {
        var query = new GetWarehousesPaginatedQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortField = sortField,
            SortOrder = sortOrder,
            FilterCode = filterCode,
            FilterName = filterName,
            FilterAddress = filterAddress,
            FilterStatus = filterStatus,
            FilterDefault = filterDefault
        };
        var result = await _mediator.Send(query);
        return Ok(Result<PagedResult<WarehouseDto>>.Succeed(result));
    }

    /// <summary>
    /// ID'ye göre depo detayını getirir
    /// </summary>
    /// <param name="id">Depo ID</param>
    /// <returns>Depo detayı</returns>
    [HttpGet("warehouses/{id}")]
    [ProducesResponseType(typeof(Result<WarehouseDetailDto>), 200)]
    public async Task<IActionResult> GetWarehouseById(int id)
    {
        var result = await _mediator.Send(new GetWarehouseByIdQuery { Id = id });
        if (result == null)
        {
            return NotFound(Result<WarehouseDetailDto>.Failure("Warehouse not found."));
        }
        return Ok(Result<WarehouseDetailDto>.Succeed(result));
    }

    /// <summary>
    /// Yeni depo oluşturur
    /// </summary>
    /// <param name="dto">Depo bilgileri</param>
    /// <returns>Oluşturulan depo ID</returns>
    [HttpPost("warehouses")]
    [ProducesResponseType(typeof(Result<int>), 200)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto dto)
    {
        var command = new CreateWarehouseCommand
        {
            Code = dto.Code,
            Name = dto.Name,
            Address = dto.Address,
            IsDefault = dto.IsDefault
        };
        var result = await _mediator.Send(command);
        return Ok(Result<int>.Succeed(result));
    }

    /// <summary>
    /// Depo günceller
    /// </summary>
    /// <param name="id">Depo ID</param>
    /// <param name="dto">Güncel depo bilgileri</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPut("warehouses/{id}")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseDto dto)
    {
        var command = new UpdateWarehouseCommand
        {
            Id = id,
            Code = dto.Code,
            Name = dto.Name,
            Address = dto.Address,
            IsActive = dto.IsActive,
            IsDefault = dto.IsDefault
        };
        var result = await _mediator.Send(command);
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Depo siler (soft delete)
    /// </summary>
    /// <param name="id">Depo ID</param>
    /// <returns>İşlem sonucu</returns>
    [HttpDelete("warehouses/{id}")]
    [ProducesResponseType(typeof(Result<bool>), 200)]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        var command = new DeleteWarehouseCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Siparişteki ürünlerin stok durumunu kontrol eder
    /// </summary>
    /// <param name="orderId">Sipariş Id</param>
    /// <returns>Stok durum listesi</returns>
    [HttpGet("check-order-stock/{orderId}")]
    [ProducesResponseType(typeof(Result<List<ProductStockStatusDto>>), 200)]
    public async Task<IActionResult> CheckOrderStock(long orderId)
    {
        var result = await _mediator.Send(new CheckOrderStockQuery { OrderId = orderId });
        return Ok(Result<List<ProductStockStatusDto>>.Succeed(result));
    }

    /// <summary>
    /// Stok düzenleme için örnek Excel şablonunu indirir.
    /// </summary>
    /// <returns>Excel dosyası</returns>
    [HttpGet("stock-adjustment/template")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public async Task<IActionResult> GetStockAdjustmentTemplate()
    {
        var fileContent = await _mediator.Send(new CWI.Application.Features.Inventory.Queries.GetStockAdjustmentTemplate.GetStockAdjustmentTemplateQuery());
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StockAdjustmentTemplate.xlsx");
    }
}
