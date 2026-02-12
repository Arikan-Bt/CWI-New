using CWI.Application.Common.Models;
using CWI.Application.DTOs.Products;
using CWI.Application.Features.ProductSalesPrices.Commands.CreateProductSalesPrice;
using CWI.Application.Features.ProductSalesPrices.Commands.DeleteProductSalesPrice;
using CWI.Application.Features.ProductSalesPrices.Commands.UpdateProductSalesPrice;
using CWI.Application.Features.ProductSalesPrices.Queries.GetProductSalesPrices;
using CWI.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize(Permissions.SalesPrices.View)]
[ApiController]
[Route("[controller]")]
public class ProductSalesPricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductSalesPricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? customerId, [FromQuery] int? productId)
    {
        var result = await _mediator.Send(new GetProductSalesPricesQuery { CustomerId = customerId, ProductId = productId });
        return Ok(Result<List<ProductSalesPriceDto>>.Succeed(result));
    }

    [Authorize(Permissions.SalesPrices.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductSalesPriceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(Result<int>.Succeed(result));
    }

    [Authorize(Permissions.SalesPrices.Edit)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductSalesPriceCommand command)
    {
        if (id != command.Id) return BadRequest(Result.Failure("ID mismatch."));
        var result = await _mediator.Send(command);
        return Ok(Result<int>.Succeed(result));
    }

    [Authorize(Permissions.SalesPrices.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteProductSalesPriceCommand { Id = id });
        return Ok(Result<int>.Succeed(result));
    }
}
