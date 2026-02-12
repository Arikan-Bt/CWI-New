using CWI.Application.Common.Models;
using CWI.Application.DTOs.Products;
using CWI.Application.Features.ProductPurchasePrices.Commands.CreateProductPurchasePrice;
using CWI.Application.Features.ProductPurchasePrices.Commands.DeleteProductPurchasePrice;
using CWI.Application.Features.ProductPurchasePrices.Commands.UpdateProductPurchasePrice;
using CWI.Application.Features.ProductPurchasePrices.Queries.GetProductPurchasePrices;
using CWI.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize(Permissions.PurchasePrices.View)]
[ApiController]
[Route("[controller]")]
public class ProductPurchasePricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductPurchasePricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? vendorId, [FromQuery] int? productId)
    {
        var result = await _mediator.Send(new GetProductPurchasePricesQuery { VendorId = vendorId, ProductId = productId });
        return Ok(Result<List<ProductPurchasePriceDto>>.Succeed(result));
    }

    [Authorize(Permissions.PurchasePrices.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductPurchasePriceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(Result<int>.Succeed(result));
    }

    [Authorize(Permissions.PurchasePrices.Edit)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductPurchasePriceCommand command)
    {
        if (id != command.Id) return BadRequest(Result.Failure("ID mismatch."));
        var result = await _mediator.Send(command);
        return Ok(Result<int>.Succeed(result));
    }

    [Authorize(Permissions.PurchasePrices.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteProductPurchasePriceCommand { Id = id });
        return Ok(Result<int>.Succeed(result));
    }
}
