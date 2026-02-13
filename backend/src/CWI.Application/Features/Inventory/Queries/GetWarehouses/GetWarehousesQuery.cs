using CWI.Application.DTOs.Common;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Common.Caching;
using CWI.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Inventory.Queries.GetWarehouses;

/// <summary>
/// Depoları paginated listele (yönetim paneli için)
/// </summary>
public record GetWarehousesPaginatedQuery : PagedRequest, IRequest<PagedResult<WarehouseDto>>
{
    public string? FilterCode { get; init; }
    public string? FilterName { get; init; }
    public string? FilterAddress { get; init; }
    public string? FilterStatus { get; init; }
    public string? FilterDefault { get; init; }
}

public class GetWarehousesPaginatedQueryHandler : IRequestHandler<GetWarehousesPaginatedQuery, PagedResult<WarehouseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWarehousesPaginatedQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<WarehouseDto>> Handle(GetWarehousesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Warehouse, int>();
        var query = repo.AsQueryable().AsNoTracking();
        var isAsc = (request.SortOrder ?? 1) == 1;

        // Search: Code veya Name'de ara
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(w => w.Code.ToLower().Contains(search) || 
                                     w.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterCode))
        {
            var filter = request.FilterCode.ToLower();
            query = query.Where(w => w.Code.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterName))
        {
            var filter = request.FilterName.ToLower();
            query = query.Where(w => w.Name.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterAddress))
        {
            var filter = request.FilterAddress.ToLower();
            query = query.Where(w => w.Address != null && w.Address.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterStatus))
        {
            var filter = request.FilterStatus.ToLower();
            query = query.Where(w => (w.IsActive ? "active" : "inactive").Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterDefault))
        {
            var filter = request.FilterDefault.ToLower();
            query = query.Where(w => (w.IsDefault ? "default" : "no").Contains(filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortField?.ToLowerInvariant() switch
        {
            "code" => isAsc ? query.OrderBy(w => w.Code) : query.OrderByDescending(w => w.Code),
            "name" => isAsc ? query.OrderBy(w => w.Name) : query.OrderByDescending(w => w.Name),
            "isactive" => isAsc ? query.OrderBy(w => w.IsActive) : query.OrderByDescending(w => w.IsActive),
            "isdefault" => isAsc ? query.OrderBy(w => w.IsDefault) : query.OrderByDescending(w => w.IsDefault),
            "status" => isAsc ? query.OrderBy(w => w.IsActive) : query.OrderByDescending(w => w.IsActive),
            _ => query.OrderBy(w => w.Name)
        };

        var warehouses = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive,
                IsDefault = w.IsDefault
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<WarehouseDto>
        {
            Items = warehouses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Tüm aktif depoları listele (dropdown için - mevcut fonksiyonalite korunuyor)
/// </summary>
public class GetWarehousesQuery : IRequest<List<WarehouseDto>>, ICacheableQuery
{
    public string CacheKey => CachePrefixes.LookupWarehouses;
    public TimeSpan SlidingExpiration => TimeSpan.FromMinutes(2);
    public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    public bool BypassCache { get; init; }
    public bool IsUserScoped => false;

    public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWarehousesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
        {
            var warehouses = await _unitOfWork.Repository<Warehouse, int>()
                .AsQueryable()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Address = w.Address,
                    IsActive = w.IsActive,
                    IsDefault = w.IsDefault
                })
                .ToListAsync(cancellationToken);

            return warehouses;
        }
    }
}

/// <summary>
/// Depo DTO (genişletilmiş)
/// </summary>
public class WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
