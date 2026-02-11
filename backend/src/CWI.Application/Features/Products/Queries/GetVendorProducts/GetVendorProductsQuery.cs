using CWI.Application.DTOs.Products;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Products.Queries.GetVendorProducts;

public class GetVendorProductsQuery : IRequest<ProductListResponse>
{
    public string? SearchTerm { get; set; }
    public List<int>? BrandIds { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;

    public class GetVendorProductsQueryHandler : IRequestHandler<GetVendorProductsQuery, ProductListResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetVendorProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductListResponse> Handle(GetVendorProductsQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.Repository<Product, int>();
            
            var query = productRepo.AsQueryable()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Brand)
                .Include(x => x.Prices)
                .Include(x => x.Images)
                .Include(x => x.InventoryItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(term) || x.Sku.ToLower().Contains(term));
            }

            if (request.BrandIds != null && request.BrandIds.Any())
            {
                query = query.Where(x => x.BrandId.HasValue && request.BrandIds.Contains(x.BrandId.Value));
            }

            // DEBUG: Removing Console.WriteLine and short-circuit
            // Taking simplified approach for Prices to avoid SQL translation errors

            var totalCount = await query.CountAsync(cancellationToken);

            var products = await query
                .OrderBy(x => x.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    Sku = x.Sku,
                    Name = x.Name,
                    BrandName = x.Brand != null ? x.Brand.Name : string.Empty,
                    PurchasePrice = x.Prices.OrderByDescending(p => p.ValidFrom).Select(p => p.UnitPrice).FirstOrDefault(),

                    ImageUrl = x.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                    StockCount = x.InventoryItems.Sum(i => i.QuantityOnHand - i.QuantityReserved),
                    IsInStock = x.InventoryItems.Sum(i => i.QuantityOnHand - i.QuantityReserved) > 0
                })
                .ToListAsync(cancellationToken);

            return new ProductListResponse
            {
                Data = products,
                TotalCount = totalCount
            };
        }
    }
}
