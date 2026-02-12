using CWI.Application.Features.Products.Queries.GetBrands;
using CWI.Application.Features.Products.Queries.GetVendorProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Vendor rolündeki kullanıcılar için ürünleri listeler
    /// </summary>
    [HttpGet("vendor-products")]
    public async Task<IActionResult> GetVendorProducts([FromQuery] GetVendorProductsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Ürün detayını getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductDetail(int id)
    {
        var result = await _mediator.Send(new CWI.Application.Features.Products.Queries.GetProductDetail.GetProductDetailQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }


    /// <summary>
    /// Tüm markaları listeler
    /// </summary>
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        var result = await _mediator.Send(new GetBrandsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Ürüne görsel yükler
    /// </summary>
    [HttpPost("{id}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        var result = await _mediator.Send(new CWI.Application.Features.Products.Commands.UploadProductImage.UploadProductImageCommand { ProductId = id, File = file });
        return Ok(result);
    }

    /// <summary>
    /// Ürün görselini siler
    /// </summary>
    [HttpDelete("{id}/images")]
    public async Task<IActionResult> DeleteImage(int id, [FromBody] DeleteImageRequest request)
    {
        var result = await _mediator.Send(new CWI.Application.Features.Products.Commands.DeleteProductImage.DeleteProductImageCommand { ProductId = id, ImageUrl = request.ImageUrl });
        return Ok(result);
    }

    public class DeleteImageRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
