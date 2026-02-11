using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Products;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public class GetStockReportQuery : IRequest<StockReportResponse>
{
    public StockReportRequest Request { get; set; } = new();
}

public class GetStockReportHandler : IRequestHandler<GetStockReportQuery, StockReportResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockReportHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StockReportResponse> Handle(GetStockReportQuery request, CancellationToken cancellationToken)
    {
        var productsQuery = _unitOfWork.Repository<Product, int>().AsQueryable();
        var inventoryQuery = _unitOfWork.Repository<InventoryItem, long>().AsQueryable();
        var purchaseOrderItemQuery = _unitOfWork.Repository<PurchaseOrderItem, long>().AsQueryable();

        var baseQuery = productsQuery
            .Select(p => new
            {
                Product = p,
                Stock = inventoryQuery
                    .Where(i => i.ProductId == p.Id)
                    .Select(i => (int?)i.QuantityOnHand)
                    .Sum() ?? 0,
                Reserved = inventoryQuery
                    .Where(i => i.ProductId == p.Id)
                    .Select(i => (int?)i.QuantityReserved)
                    .Sum() ?? 0,
                IncomingStock = purchaseOrderItemQuery
                    .Where(i => i.ProductId == p.Id && i.Quantity > i.ReceivedQuantity)
                    .Select(i => (int?)(i.Quantity - i.ReceivedQuantity))
                    .Sum() ?? 0,
                ShelfNumber = inventoryQuery
                    .Where(i => i.ProductId == p.Id)
                    .Select(i => i.ShelfNumber)
                    .FirstOrDefault(),
            })
            .Where(x => x.Stock != 0 || x.Reserved != 0 || x.IncomingStock != 0);

        if (!string.IsNullOrEmpty(request.Request.Brand))
        {
            baseQuery = baseQuery.Where(x => x.Product.Brand != null && x.Product.Brand.Name == request.Request.Brand);
        }

        if (!string.IsNullOrEmpty(request.Request.SearchValue))
        {
            var search = request.Request.SearchValue.ToLower();
            baseQuery = baseQuery.Where(x =>
                x.Product.Sku.ToLower().Contains(search) || x.Product.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(request.Request.FilterItemCode))
        {
            var filter = request.Request.FilterItemCode.ToLower();
            baseQuery = baseQuery.Where(x => x.Product.Sku.ToLower().Contains(filter));
        }

        if (!string.IsNullOrEmpty(request.Request.FilterItemDescription))
        {
            var filter = request.Request.FilterItemDescription.ToLower();
            baseQuery = baseQuery.Where(x => x.Product.Name.ToLower().Contains(filter));
        }

        if (!string.IsNullOrEmpty(request.Request.FilterShelfNumber))
        {
            var filter = request.Request.FilterShelfNumber.ToLower();
            baseQuery = baseQuery.Where(x => x.ShelfNumber != null && x.ShelfNumber.ToLower().Contains(filter));
        }

        var pricesRepo = _unitOfWork.Repository<ProductPrice>();

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        if (!string.IsNullOrEmpty(request.Request.SortField))
        {
            var isAsc = request.Request.SortOrder == 1;
            switch (request.Request.SortField)
            {
                case "itemCode":
                    baseQuery = isAsc ? baseQuery.OrderBy(i => i.Product.Sku) : baseQuery.OrderByDescending(i => i.Product.Sku);
                    break;
                case "itemDescription":
                    baseQuery = isAsc ? baseQuery.OrderBy(i => i.Product.Name) : baseQuery.OrderByDescending(i => i.Product.Name);
                    break;
                case "stock":
                    baseQuery = isAsc ? baseQuery.OrderBy(i => i.Stock) : baseQuery.OrderByDescending(i => i.Stock);
                    break;
                case "reserved":
                    baseQuery = isAsc ? baseQuery.OrderBy(i => i.Reserved) : baseQuery.OrderByDescending(i => i.Reserved);
                    break;
                case "available":
                    baseQuery = isAsc
                        ? baseQuery.OrderBy(i => i.Stock - i.Reserved)
                        : baseQuery.OrderByDescending(i => i.Stock - i.Reserved);
                    break;
                case "incomingStock":
                    baseQuery = isAsc ? baseQuery.OrderBy(i => i.IncomingStock) : baseQuery.OrderByDescending(i => i.IncomingStock);
                    break;
                case "shelfNumber":
                    baseQuery = isAsc ? baseQuery.OrderBy(i => i.ShelfNumber) : baseQuery.OrderByDescending(i => i.ShelfNumber);
                    break;
                default:
                    baseQuery = baseQuery.OrderBy(i => i.Product.Sku);
                    break;
            }
        }
        else
        {
            baseQuery = baseQuery.OrderBy(i => i.Product.Sku);
        }

        var items = await baseQuery
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .Select(x => new StockReportItemDto
            {
                ProductId = x.Product.Id,
                ItemCode = x.Product.Sku,
                ItemDescription = x.Product.Name,
                Stock = x.Stock,
                Reserved = x.Reserved,
                Available = x.Stock - x.Reserved,
                IncomingStock = x.IncomingStock,
                Brand = x.Product.Brand != null ? x.Product.Brand.Name : string.Empty,
                Picture = $"https://cdn.arikantime.com/ProductImages/{x.Product.Sku}.jpg",
                SpecialNote = x.Product.Notes.OrderByDescending(n => n.CreatedAt).Select(n => n.Content).FirstOrDefault(),
                ShelfNumber = x.ShelfNumber,
                RetailSalesPrice = pricesRepo.AsQueryable()
                    .Where(pp => pp.ProductId == x.Product.Id && pp.IsActive)
                    .OrderByDescending(pp => pp.ValidFrom)
                    .Select(pp => pp.UnitPrice)
                    .FirstOrDefault(),
                Attributes = x.Product.Attributes
            }).ToListAsync(cancellationToken);

        await PopulateDetailsAsync(items, cancellationToken);

        return new StockReportResponse { Items = items, TotalCount = totalCount };
    }

    private async Task PopulateDetailsAsync(List<StockReportItemDto> items, CancellationToken cancellationToken)
    {
        if (!items.Any())
        {
            return;
        }

        var productIds = items.Select(x => x.ProductId).Distinct().ToList();

            // StockMovement queries removed

        var fallbackAdjustmentItems = await _unitOfWork.Repository<StockAdjustmentItem, long>().AsQueryable()
            .Include(x => x.StockAdjustment)
            .Include(x => x.Product)
            .Where(x => productIds.Contains(x.ProductId))
            .OrderByDescending(x => x.StockAdjustment.AdjustmentDate)
            .ToListAsync(cancellationToken);

            // GoodsReceipt query removed as not used

        var fallbackSales = await _unitOfWork.Repository<OrderItem, long>().AsQueryable()
            .Include(x => x.Product)
            .Include(x => x.Order)
            .Where(x => productIds.Contains(x.ProductId) && x.Order.Status == OrderStatus.Shipped)
            .OrderByDescending(x => x.Order.ShippedAt ?? x.Order.OrderedAt)
            .ToListAsync(cancellationToken);

        var fallbackStatus = await _unitOfWork.Repository<OrderItem, long>().AsQueryable()
            .Include(x => x.Product)
            .Include(x => x.Order)
            .Where(x => productIds.Contains(x.ProductId)
                        && (x.Order.Status == OrderStatus.Pending
                            || x.Order.Status == OrderStatus.Approved
                            || x.Order.Status == OrderStatus.PreOrder
                            || x.Order.Status == OrderStatus.PackedAndWaitingShipment))
            .OrderByDescending(x => x.Order.OrderedAt)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            // StockMovement legacy block removed to favor live tables (GoodsReceipt, Orders, Adjustments)

            var legacyDetails = new List<StockReportDetailDto>();

            legacyDetails.AddRange(fallbackAdjustmentItems
                .Where(x => x.ProductId == item.ProductId)
                .Select(x =>
                {
                    var isPurchaseReceive = !string.IsNullOrEmpty(x.ReceivingNumber) ||
                                            (x.StockAdjustment.Description != null && x.StockAdjustment.Description.Contains("Purchase Receive"));

                    return new StockReportDetailDto
                    {
                        ShelfNumber = x.ShelfNumber,
                        PackList = x.PackList,
                        ReceiveDate = x.StockAdjustment.AdjustmentDate,
                        OccurredAt = x.StockAdjustment.AdjustmentDate,
                        Quantity = x.NewQuantity - x.OldQuantity,
                        MovementType = isPurchaseReceive ? StockMovementType.PurchaseReceive.ToString() : StockMovementType.Adjustment.ToString(),
                        MovementGroup = isPurchaseReceive ? "Stock Details" : "Adjustment",
                        SourceDocumentType = "StockAdjustment",
                        ReferenceNo = isPurchaseReceive ? x.ReceivingNumber : x.StockAdjustmentId.ToString(),
                        WarehouseId = x.WarehouseId,
                        SupplierName = x.SupplierName
                    };
                }));

            legacyDetails.AddRange(fallbackSales
                .Where(x => x.ProductId == item.ProductId)
                .Select(x => new StockReportDetailDto
                {
                    Quantity = -x.Quantity,
                    ReceiveDate = x.Order.ShippedAt ?? x.Order.OrderedAt,
                    OccurredAt = x.Order.ShippedAt ?? x.Order.OrderedAt,
                    MovementType = StockMovementType.Sale.ToString(),
                    MovementGroup = "Stock Details",
                    SourceDocumentType = "Order",
                    ReferenceNo = x.Order.OrderNumber,
                    WarehouseId = x.WarehouseId
                }));

            legacyDetails.AddRange(fallbackStatus
                .Where(x => x.ProductId == item.ProductId)
                .Select(x => new StockReportDetailDto
                {
                    Quantity = x.Quantity,
                    ReceiveDate = x.Order.OrderedAt,
                    OccurredAt = x.Order.OrderedAt,
                    MovementType = StockMovementType.Reserve.ToString(),
                    MovementGroup = "Status",
                    SourceDocumentType = "Order",
                    ReferenceNo = x.Order.OrderNumber,
                    WarehouseId = x.WarehouseId
                }));

            item.Details = legacyDetails
                .OrderByDescending(x => x.OccurredAt ?? x.ReceiveDate)
                .ToList();

            if (!item.Details.Any() && !string.IsNullOrEmpty(item.ShelfNumber))
            {
                item.Details.Add(new StockReportDetailDto
                {
                    ShelfNumber = item.ShelfNumber,
                    Quantity = item.Stock,
                    MovementType = StockMovementType.Adjustment.ToString(),
                    MovementGroup = "Adjustment",
                    SourceDocumentType = "InventorySnapshot"
                });
            }
        }
    }

    private static StockReportDetailDto ToDetail(StockMovement movement)
    {
        var quantity = movement.QuantityDeltaOnHand != 0
            ? movement.QuantityDeltaOnHand
            : movement.QuantityDeltaReserved;

        return new StockReportDetailDto
        {
            ShelfNumber = movement.ShelfNumber,
            PackList = movement.PackList,
            ReceiveDate = movement.OccurredAt,
            OccurredAt = movement.OccurredAt,
            Quantity = quantity,
            MovementType = movement.MovementType.ToString(),
            MovementGroup = MapMovementGroup(movement.MovementType),
            ReferenceNo = movement.ReferenceNo,
            SourceDocumentType = movement.SourceDocumentType,
            WarehouseId = movement.WarehouseId,
            WarehouseName = movement.Warehouse?.Name,
            SupplierName = movement.SupplierName
        };
    }

    private static string MapMovementGroup(StockMovementType movementType)
    {
        return movementType switch
        {
            StockMovementType.Adjustment => "Adjustment",
            StockMovementType.PurchaseReceive or StockMovementType.Sale or StockMovementType.SaleRevert => "Stock Details",
            StockMovementType.Reserve or StockMovementType.Unreserve => "Status",
            _ => "Status"
        };
    }
}
