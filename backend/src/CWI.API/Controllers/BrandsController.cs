using CWI.Application.Common.Models;
using CWI.Application.DTOs.Brands;
using CWI.Application.Features.Brands.Commands.CreateBrand;
using CWI.Application.Features.Brands.Commands.DeleteBrand;
using CWI.Application.Features.Brands.Commands.UpdateBrand;
using CWI.Application.Features.Brands.Queries.GetBrandById;
using CWI.Application.Features.Brands.Queries.GetBrands;
using CWI.Application.Features.Brands.Queries.GetBrandProducts;
using CWI.Application.Features.Brands.Commands.UpdateBrandProducts;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

/// <summary>
/// Marka yönetimi API controller
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Marka listesini getirir (sayfalama, arama ve ProjectType filtresi destekli)
    /// </summary>
    /// <param name="page">Sayfa numarası (1-indexed)</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="searchText">Arama metni</param>
    /// <param name="projectType">Proje tipi filtresi (opsiyonel)</param>
    /// <param name="sortField">Sıralama alanı</param>
    /// <param name="sortOrder">Sıralama yönü (1: asc, -1: desc)</param>
    /// <returns>Marka listesi</returns>
    [HttpGet]
    public async Task<IActionResult> GetBrands(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] ProjectType? projectType = null,
        [FromQuery] string? sortField = null,
        [FromQuery] int sortOrder = 1,
        [FromQuery] string? filterCode = null,
        [FromQuery] string? filterName = null,
        [FromQuery] string? filterProjectType = null,
        [FromQuery] string? filterSortOrder = null,
        [FromQuery] string? filterStatus = null)
    {
        var result = await _mediator.Send(new GetBrandsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchText = searchText,
            ProjectTypeFilter = projectType,
            SortField = sortField,
            SortOrder = sortOrder,
            FilterCode = filterCode,
            FilterName = filterName,
            FilterProjectType = filterProjectType,
            FilterSortOrder = filterSortOrder,
            FilterStatus = filterStatus
        });

        return Ok(Result<BrandListResponse>.Succeed(result));
    }

    /// <summary>
    /// Belirtilen Id'ye sahip markayı getirir
    /// </summary>
    /// <param name="id">Marka Id</param>
    /// <returns>Marka detayı</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrandById(int id)
    {
        var result = await _mediator.Send(new GetBrandByIdQuery { Id = id });
        if (result == null) return NotFound(Result<BrandDetailDto>.Failure("Brand not found."));
        return Ok(Result<BrandDetailDto>.Succeed(result));
    }

    /// <summary>
    /// Yeni marka oluşturur
    /// </summary>
    /// <param name="request">Marka oluşturma verileri</param>
    /// <returns>Oluşturulan marka</returns>
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto request)
    {
        var result = await _mediator.Send(new CreateBrandCommand { Data = request });
        return Ok(Result<BrandDetailDto>.Succeed(result));
    }

    /// <summary>
    /// Mevcut markayı günceller
    /// </summary>
    /// <param name="id">Marka Id</param>
    /// <param name="request">Marka güncelleme verileri</param>
    /// <returns>Güncellenen marka</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(int id, [FromBody] UpdateBrandDto request)
    {
        if (id != request.Id) return BadRequest(Result.Failure("ID mismatch."));
        var result = await _mediator.Send(new UpdateBrandCommand { Data = request });
        return Ok(Result<BrandDetailDto>.Succeed(result));
    }

    /// <summary>
    /// Markayı siler (soft delete)
    /// </summary>
    /// <param name="id">Marka Id</param>
    /// <returns>İşlem sonucu</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var result = await _mediator.Send(new DeleteBrandCommand { Id = id });
        return Ok(Result<bool>.Succeed(result));
    }

    /// <summary>
    /// Markaya ait ürünleri getirir
    /// </summary>
    /// <param name="id">Marka Id</param>
    /// <returns>Marka ürünleri listesi</returns>
    [HttpGet("{id}/products")]
    public async Task<IActionResult> GetBrandProducts(int id)
    {
        var result = await _mediator.Send(new GetBrandProductsQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Markaya ürünleri atar
    /// </summary>
    /// <param name="id">Marka Id</param>
    /// <param name="productIds">Ürün Id listesi</param>
    /// <returns>İşlem sonucu</returns>
    [HttpPut("{id}/products")]
    public async Task<IActionResult> UpdateBrandProducts(int id, [FromBody] List<int> productIds)
    {
        var result = await _mediator.Send(new UpdateBrandProductsCommand(id, productIds));
        return Ok(result);
    }
}
