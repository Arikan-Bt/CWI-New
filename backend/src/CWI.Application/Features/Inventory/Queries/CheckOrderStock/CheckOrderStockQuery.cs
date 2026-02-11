using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Inventory.Queries.CheckOrderStock;

public class CheckOrderStockQuery : IRequest<List<ProductStockStatusDto>>
{
    public long OrderId { get; set; }

    public class CheckOrderStockQueryHandler : IRequestHandler<CheckOrderStockQuery, List<ProductStockStatusDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckOrderStockQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductStockStatusDto>> Handle(CheckOrderStockQuery request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Repository<Order, long>()
                .AsQueryable()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.InventoryItems)
                            .ThenInclude(inv => inv.Warehouse)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) return new List<ProductStockStatusDto>();

            var result = new List<ProductStockStatusDto>();

            foreach (var item in order.Items)
            {
                if (item.Product == null) continue;

                // Sadece aktif ve stoğu olan depoları getir
                var availableWarehouses = item.Product.InventoryItems
                    .Where(inv => inv.QuantityAvailable > 0 && inv.Warehouse.IsActive)
                    .Select(inv => new WarehouseStockDto
                    {
                        WarehouseId = inv.WarehouseId,
                        WarehouseName = inv.Warehouse.Name,
                        AvailableQty = inv.QuantityAvailable
                    })
                    .OrderByDescending(w => w.AvailableQty)
                    .ToList();

                var stockStatus = new ProductStockStatusDto
                {
                    ProductCode = item.Product.Sku,
                    ProductName = item.Product.Name,
                    RequiredQty = item.Quantity,
                    Warehouses = availableWarehouses
                };

                result.Add(stockStatus);
            }

            return result;
        }
    }
}

public class ProductStockStatusDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int RequiredQty { get; set; }
    public List<WarehouseStockDto> Warehouses { get; set; } = new();
    
    public bool HasMultipleWarehouses => Warehouses.Count > 1;
    public bool HasSufficientStock => Warehouses.Sum(w => w.AvailableQty) >= RequiredQty;
}

public class WarehouseStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int AvailableQty { get; set; }
}
