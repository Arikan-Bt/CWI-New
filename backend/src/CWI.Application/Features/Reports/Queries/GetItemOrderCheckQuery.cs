using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Products;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;

namespace CWI.Application.Features.Reports.Queries;

public class GetItemOrderCheckQuery : IRequest<ItemOrderCheckResponse>
{
    public ItemOrderCheckRequest Request { get; set; } = new();
}

public class GetItemOrderCheckQueryHandler : IRequestHandler<GetItemOrderCheckQuery, ItemOrderCheckResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetItemOrderCheckQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ItemOrderCheckResponse> Handle(GetItemOrderCheckQuery query, CancellationToken cancellationToken)
    {
        var sku = query.Request.Sku;

        var productRepo = _unitOfWork.Repository<Product, int>();
        var orderItemRepo = _unitOfWork.Repository<OrderItem, long>();
        var purchaseOrderItemRepo = _unitOfWork.Repository<PurchaseOrderItem, long>();
        var inventoryRepo = _unitOfWork.Repository<InventoryItem, long>();

        var product = await productRepo.AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

        if (product == null)
            return new ItemOrderCheckResponse();

        // Summary
        var purchaseQty = await purchaseOrderItemRepo.AsQueryable()
            .AsNoTracking()
            .Where(poi => poi.ProductId == product.Id)
            .SumAsync(poi => (decimal)poi.Quantity, cancellationToken);

        var salesQty = await orderItemRepo.AsQueryable()
            .AsNoTracking()
            .Where(oi => oi.ProductId == product.Id)
            .SumAsync(oi => (decimal)oi.Quantity, cancellationToken);

        var inventory = await inventoryRepo.AsQueryable()
            .AsNoTracking()
            .Where(ii => ii.ProductId == product.Id)
            .GroupBy(ii => ii.ProductId)
            .Select(g => new
            {
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var summary = new ItemOrderSummaryDto
        {
            ItemCode = sku,
            PurchaseQty = purchaseQty,
            SalesQty = salesQty,
            WareHouseQty = inventory?.OnHand ?? 0,
            ReserveQty = inventory?.Reserved ?? 0,
            AvailableQty = (inventory?.OnHand ?? 0) - (inventory?.Reserved ?? 0),
            IncomingStock = await purchaseOrderItemRepo.AsQueryable()
                .AsNoTracking()
                .Include(x => x.PurchaseOrder)
                .Where(poi => poi.ProductId == product.Id && 
                              poi.Quantity > poi.ReceivedQuantity)
                .SumAsync(poi => (decimal)(poi.Quantity - poi.ReceivedQuantity), cancellationToken)
        };

        // Movements (Item Check Status)
        IQueryable<OrderItem> movementsQuery = orderItemRepo.AsQueryable()
            .AsNoTracking()
            .Include(oi => oi.Order)
                .ThenInclude(o => o.Customer);

        // Eğer kullanıcı yönetici ise tüm hareketleri görebilir.
        // Yönetici değilse ve bağlı bir müşterisi varsa sadece o müşterinin hareketlerini görür.
        if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
        {
            movementsQuery = movementsQuery.Where(oi => oi.Order.CustomerId == _currentUserService.LinkedCustomerId.Value);
        }

        var totalMovements = await movementsQuery
            .Where(oi => oi.ProductId == product.Id)
            .CountAsync(cancellationToken);

        var movementsData = await movementsQuery
            .Where(oi => oi.ProductId == product.Id)
            .OrderByDescending(oi => oi.Order.OrderedAt)
            .Skip((query.Request.Page - 1) * query.Request.PageSize)
            .Take(query.Request.PageSize)
            .Select(oi => new
            {
                oi.Order.Status,
                oi.Order.Id,
                AccountName = oi.Order.Customer != null ? oi.Order.Customer.Name : "Unknown",
                oi.Quantity
            })
            .ToListAsync(cancellationToken);

        var movements = movementsData.Select(m => new ItemMovementDto
        {
            Status = m.Status switch
            {
                CWI.Domain.Enums.OrderStatus.Draft => "Draft",
                CWI.Domain.Enums.OrderStatus.Pending => "Pending",
                CWI.Domain.Enums.OrderStatus.Approved => "Order",
                CWI.Domain.Enums.OrderStatus.Shipped => "Shipped",
                CWI.Domain.Enums.OrderStatus.Canceled => "Canceled",
                CWI.Domain.Enums.OrderStatus.PreOrder => "Pre Order",
                CWI.Domain.Enums.OrderStatus.PackedAndWaitingShipment => "Packed & Waiting Shipment",
                _ => m.Status.ToString()
            },
            Url = $"/pages/reports/orders/{m.Id}/details",
            Account = m.AccountName,
            Qty = m.Quantity,
            ItemCode = product.Sku
        }).ToList();

        return new ItemOrderCheckResponse
        {
            Summary = summary,
            Movements = movements,
            TotalMovements = totalMovements
        };
    }
}
