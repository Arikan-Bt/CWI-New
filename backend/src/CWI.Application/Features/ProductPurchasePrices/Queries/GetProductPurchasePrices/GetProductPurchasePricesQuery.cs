using CWI.Application.DTOs.Products;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.ProductPurchasePrices.Queries.GetProductPurchasePrices;

public class GetProductPurchasePricesQuery : IRequest<List<ProductPurchasePriceDto>>
{
    public int? VendorId { get; set; }
    public int? ProductId { get; set; }

    public class GetProductPurchasePricesQueryHandler : IRequestHandler<GetProductPurchasePricesQuery, List<ProductPurchasePriceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductPurchasePricesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductPurchasePriceDto>> Handle(GetProductPurchasePricesQuery request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<StockAdjustmentItem, long>();
            
            var query = repository.AsQueryable()
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.StockAdjustment)
                .Where(x => !string.IsNullOrEmpty(x.ReceivingNumber) || (x.StockAdjustment.Description != null && x.StockAdjustment.Description.Contains("Purchase Receive")))
                .AsQueryable();

            if (request.VendorId.HasValue)
            {
                // Note: If vendor id is needed, we might need to join with Customer table or use SupplierName comparison.
                // Assuming VendorId filter is not strictly required if we want to show 'all movements' as requested.
                // But if needed, we'd need more logic. For now, following 'show all movements' request.
            }

            if (request.ProductId.HasValue)
            {
                query = query.Where(x => x.ProductId == request.ProductId.Value);
            }

            var prices = await query
                .OrderByDescending(x => x.StockAdjustment != null ? x.StockAdjustment.AdjustmentDate : DateTime.MinValue)
                .Take(1000)
                .Select(x => new ProductPurchasePriceDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductSku = x.Product != null ? x.Product.Sku : string.Empty,
                    ProductName = x.Product != null ? x.Product.Name : string.Empty,
                    VendorName = x.SupplierName ?? string.Empty,
                    Price = x.Price ?? 0,
                    CurrencyCode = x.Currency ?? "USD",
                    ValidFrom = x.StockAdjustment != null ? x.StockAdjustment.AdjustmentDate : DateTime.UtcNow,
                    IsActive = true
                })
                .ToListAsync(cancellationToken);

            return prices;
        }
    }
}
