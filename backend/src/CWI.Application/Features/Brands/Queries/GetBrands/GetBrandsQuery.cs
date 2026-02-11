using CWI.Application.DTOs.Brands;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Products;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Brands.Queries.GetBrands;

/// <summary>
/// Marka listesi sorgusu - sayfalama, arama ve ProjectType filtresi destekli
/// </summary>
public class GetBrandsQuery : IRequest<BrandListResponse>
{
    /// <summary>Sayfa numarasý (1-indexed)</summary>
    public int Page { get; set; } = 1;

    /// <summary>Sayfa boyutu</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Arama metni</summary>
    public string? SearchText { get; set; }

    /// <summary>Proje tipi filtresi (opsiyonel - admin için)</summary>
    public ProjectType? ProjectTypeFilter { get; set; }

    /// <summary>Sýralama alaný</summary>
    public string? SortField { get; set; }

    /// <summary>Sýralama yönü (1: asc, -1: desc)</summary>
    public int SortOrder { get; set; } = 1;

    public string? FilterCode { get; set; }
    public string? FilterName { get; set; }
    public string? FilterProjectType { get; set; }
    public string? FilterSortOrder { get; set; }
    public string? FilterStatus { get; set; }
}

/// <summary>
/// GetBrandsQuery handler
/// </summary>
public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, BrandListResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetBrandsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BrandListResponse> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brandRepo = _unitOfWork.Repository<Brand, int>();
        var query = brandRepo.AsQueryable().AsNoTracking();

        if (!_currentUserService.IsAdministrator && _currentUserService.ProjectType.HasValue)
        {
            query = query.Where(x => x.ProjectType == _currentUserService.ProjectType.Value);
        }
        else if (request.ProjectTypeFilter.HasValue)
        {
            query = query.Where(x => x.ProjectType == request.ProjectTypeFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchLower = request.SearchText.ToLower();
            query = query.Where(x =>
                x.Code.ToLower().Contains(searchLower) ||
                x.Name.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterCode))
        {
            var filter = request.FilterCode.ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterName))
        {
            var filter = request.FilterName.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterProjectType))
        {
            var filter = request.FilterProjectType.ToLower();
            query = query.Where(x => x.ProjectType.ToString().ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterSortOrder) &&
            int.TryParse(request.FilterSortOrder, out var filterSortOrder))
        {
            query = query.Where(x => x.SortOrder == filterSortOrder);
        }

        if (!string.IsNullOrWhiteSpace(request.FilterStatus))
        {
            var filter = request.FilterStatus.ToLower();
            query = query.Where(x => (x.IsActive ? "active" : "inactive").Contains(filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var isAsc = request.SortOrder != -1;
        query = request.SortField?.ToLowerInvariant() switch
        {
            "code" => isAsc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "name" => isAsc ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
            "sortorder" => isAsc ? query.OrderBy(x => x.SortOrder) : query.OrderByDescending(x => x.SortOrder),
            "projecttype" => isAsc ? query.OrderBy(x => x.ProjectType) : query.OrderByDescending(x => x.ProjectType),
            "isactive" => isAsc ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
            _ => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
        };

        var brands = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new BrandDetailDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                LogoUrl = x.LogoUrl,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                ProjectType = x.ProjectType
            })
            .ToListAsync(cancellationToken);

        return new BrandListResponse
        {
            Data = brands,
            TotalCount = totalCount
        };
    }
}
