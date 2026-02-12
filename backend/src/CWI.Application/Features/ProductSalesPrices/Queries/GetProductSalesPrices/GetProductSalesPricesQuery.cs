using CWI.Application.DTOs.Products;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.ProductSalesPrices.Queries.GetProductSalesPrices;

public class GetProductSalesPricesQuery : IRequest<List<ProductSalesPriceDto>>
{
    public int? CustomerId { get; set; }
    public int? ProductId { get; set; }

    public class GetProductSalesPricesQueryHandler : IRequestHandler<GetProductSalesPricesQuery, List<ProductSalesPriceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductSalesPricesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductSalesPriceDto>> Handle(GetProductSalesPricesQuery request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<OrderItem, long>();
            
            var query = repository.AsQueryable()
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Order)
                    .ThenInclude(o => o.Customer)
                .Include(x => x.Order)
                    .ThenInclude(o => o.Currency)
                .Where(x => (x.Order.Status == OrderStatus.PackedAndWaitingShipment || x.Order.Status == OrderStatus.Shipped) && !x.Order.IsCanceled)
                .AsQueryable();

            if (request.CustomerId.HasValue)
            {
                query = query.Where(x => x.Order.CustomerId == request.CustomerId.Value);
            }

            if (request.ProductId.HasValue)
            {
                query = query.Where(x => x.ProductId == request.ProductId.Value);
            }

            var prices = await query
                .OrderByDescending(x => x.Order.OrderedAt)
                .Take(1000)
                .Select(x => new ProductSalesPriceDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductSku = x.Product != null ? x.Product.Sku : string.Empty,
                    ProductName = x.Product != null ? x.Product.Name : string.Empty,
                    CustomerId = x.Order.CustomerId,
                    CustomerName = x.Order.Customer != null ? x.Order.Customer.Name : string.Empty,
                    Price = x.UnitPrice,
                    CurrencyId = x.Order.CurrencyId,
                    CurrencyCode = x.Order.Currency != null ? x.Order.Currency.Code : "USD",
                    ValidFrom = x.Order.OrderedAt,
                    IsActive = true
                })
                .ToListAsync(cancellationToken);

            return prices;
        }
    }
}
