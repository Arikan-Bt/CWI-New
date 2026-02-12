using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Entities.Products;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CWI.Application.Features.Reports.Queries;

public class ExportStockReportQuery : IRequest<byte[]>
{
    public StockReportRequest Request { get; set; } = new();

    public class ExportStockReportHandler : IRequestHandler<ExportStockReportQuery, byte[]>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExportStockReportHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> Handle(ExportStockReportQuery request, CancellationToken cancellationToken)
        {
            IQueryable<InventoryItem> query = _unitOfWork.Repository<InventoryItem, long>().AsQueryable()
                .Include(i => i.Product)
                    .ThenInclude(p => p.Brand)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Category)
                .Include(i => i.Product)
                    .ThenInclude(p => p.SubCategory)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Color)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Notes);

            if (!string.IsNullOrEmpty(request.Request.Brand))
            {
                query = query.Where(i => i.Product.Brand != null && i.Product.Brand.Name == request.Request.Brand);
            }

            if (!string.IsNullOrEmpty(request.Request.SearchValue))
            {
                var search = request.Request.SearchValue.ToLower();
                query = query.Where(i => i.Product.Sku.ToLower().Contains(search) ||
                                         i.Product.Name.ToLower().Contains(search));
            }

            var pricesRepo = _unitOfWork.Repository<ProductPrice>();

            var items = await query.Select(i => new StockReportItemDto
            {
                ItemCode = i.Product.Sku,
                ItemDescription = i.Product.Name,
                Stock = i.QuantityOnHand,
                Reserved = i.QuantityReserved,
                Available = i.QuantityAvailable,
                Brand = i.Product.Brand != null ? i.Product.Brand.Name : string.Empty,
                Category = i.Product.Category != null ? i.Product.Category.Name : string.Empty,
                SubCategory = i.Product.SubCategory != null ? i.Product.SubCategory.Name : string.Empty,
                Color = i.Product.Color != null ? i.Product.Color.Name : string.Empty,
                Attributes = i.Product.Attributes,
                Picture = $"https://cdn.arikantime.com/ProductImages/{i.Product.Sku}.jpg",
                SpecialNote = i.Product.Notes.OrderByDescending(n => n.CreatedAt).Select(n => n.Content).FirstOrDefault(),
                ShelfNumber = i.ShelfNumber,
                RetailSalesPrice = pricesRepo.AsQueryable()
                    .Where(pp => pp.ProductId == i.ProductId && pp.IsActive)
                    .OrderByDescending(pp => pp.ValidFrom)
                    .Select(pp => pp.UnitPrice)
                    .FirstOrDefault()
            }).ToListAsync(cancellationToken);

            await PopulateDetailsAsync(items, cancellationToken);

            var attributeKeys = new HashSet<string>();
            var parsedAttributes = new Dictionary<string, Dictionary<string, string>>();

            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Attributes))
                {
                    try
                    {
                        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(item.Attributes);
                        if (dict != null)
                        {
                            var itemAttrs = new Dictionary<string, string>();
                            foreach (var kvp in dict)
                            {
                                var key = kvp.Key;
                                var val = kvp.Value?.ToString() ?? "";
                                itemAttrs[key] = val;
                                attributeKeys.Add(key);
                            }
                            parsedAttributes[item.ItemCode] = itemAttrs;
                        }
                    }
                    catch
                    {
                        // ignore invalid json in attributes
                    }
                }
            }

            var sortedAttrKeys = attributeKeys.OrderBy(k => k).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Stock Report");

            var col = 1;
            worksheet.Cells[1, col++].Value = "Image";
            worksheet.Cells[1, col++].Value = "Brand";
            worksheet.Cells[1, col++].Value = "Item Code";
            worksheet.Cells[1, col++].Value = "Description";
            worksheet.Cells[1, col++].Value = "Category";
            worksheet.Cells[1, col++].Value = "Sub Category";
            worksheet.Cells[1, col++].Value = "Color";
            worksheet.Cells[1, col++].Value = "Total Stock";
            worksheet.Cells[1, col++].Value = "Reserved";
            worksheet.Cells[1, col++].Value = "Available";
            worksheet.Cells[1, col++].Value = "Note";
            worksheet.Cells[1, col++].Value = "Detail Qty";
            worksheet.Cells[1, col++].Value = "Movement Type";
            worksheet.Cells[1, col++].Value = "Movement Group";
            worksheet.Cells[1, col++].Value = "Source Document";
            worksheet.Cells[1, col++].Value = "Reference No";
            worksheet.Cells[1, col++].Value = "Warehouse";
            worksheet.Cells[1, col++].Value = "Pack List";
            worksheet.Cells[1, col++].Value = "Supplier";
            worksheet.Cells[1, col++].Value = "Occurred At";

            foreach (var key in sortedAttrKeys)
            {
                worksheet.Cells[1, col++].Value = key;
            }

            using (var range = worksheet.Cells[1, 1, 1, col - 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            var row = 2;
            foreach (var item in items)
            {
                var details = item.Details.Any() ? item.Details : [new StockReportDetailDto()];

                foreach (var detail in details)
                {
                    col = 1;
                    worksheet.Cells[row, col++].Value = item.Picture;
                    worksheet.Cells[row, col++].Value = item.Brand;
                    worksheet.Cells[row, col++].Value = item.ItemCode;
                    worksheet.Cells[row, col++].Value = item.ItemDescription;
                    worksheet.Cells[row, col++].Value = item.Category;
                    worksheet.Cells[row, col++].Value = item.SubCategory;
                    worksheet.Cells[row, col++].Value = item.Color;
                    worksheet.Cells[row, col++].Value = item.Stock;
                    worksheet.Cells[row, col++].Value = item.Reserved;
                    worksheet.Cells[row, col++].Value = item.Available;
                    worksheet.Cells[row, col++].Value = item.SpecialNote;
                    worksheet.Cells[row, col++].Value = detail.Quantity;
                    worksheet.Cells[row, col++].Value = detail.MovementType;
                    worksheet.Cells[row, col++].Value = detail.MovementGroup;
                    worksheet.Cells[row, col++].Value = detail.SourceDocumentType;
                    worksheet.Cells[row, col++].Value = detail.ReferenceNo;
                    worksheet.Cells[row, col++].Value = detail.WarehouseName;
                    worksheet.Cells[row, col++].Value = detail.PackList;
                    worksheet.Cells[row, col++].Value = detail.SupplierName;
                    worksheet.Cells[row, col++].Value = (detail.OccurredAt ?? detail.ReceiveDate)?.ToString("dd.MM.yyyy HH:mm");

                    var itemAttrs = parsedAttributes.ContainsKey(item.ItemCode) ? parsedAttributes[item.ItemCode] : null;
                    foreach (var key in sortedAttrKeys)
                    {
                        worksheet.Cells[row, col++].Value = itemAttrs != null && itemAttrs.ContainsKey(key) ? itemAttrs[key] : "";
                    }

                    row++;
                }
            }

            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task PopulateDetailsAsync(List<StockReportItemDto> items, CancellationToken cancellationToken)
        {
            if (!items.Any())
            {
                return;
            }

            var productSkus = items.Select(x => x.ItemCode).Distinct().ToList();

            var stockMovements = await _unitOfWork.Repository<StockMovement, long>().AsQueryable()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x => productSkus.Contains(x.Product.Sku))
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);

            var movementBySku = stockMovements
                .GroupBy(x => x.Product.Sku)
                .ToDictionary(g => g.Key, g => g.ToList());

            var fallbackAdjustmentItems = await _unitOfWork.Repository<StockAdjustmentItem, long>().AsQueryable()
                .Include(x => x.StockAdjustment)
                .Include(x => x.Product)
                .Where(x => productSkus.Contains(x.Product.Sku))
                .OrderByDescending(x => x.StockAdjustment.AdjustmentDate)
                .ToListAsync(cancellationToken);

            var fallbackPurchaseReceives = await _unitOfWork.Repository<PurchaseOrderItem, long>().AsQueryable()
                .Include(x => x.Product)
                .Include(x => x.PurchaseOrder)
                .Where(x => productSkus.Contains(x.Product.Sku) && x.ReceivedQuantity > 0)
                .OrderByDescending(x => x.PurchaseOrder.OrderedAt)
                .ToListAsync(cancellationToken);

            var fallbackSales = await _unitOfWork.Repository<OrderItem, long>().AsQueryable()
                .Include(x => x.Product)
                .Include(x => x.Order)
                .Where(x => productSkus.Contains(x.Product.Sku) && 
                            (x.Order.Status == OrderStatus.Shipped || x.Order.Status == OrderStatus.PackedAndWaitingShipment))
                .OrderByDescending(x => x.Order.ShippedAt ?? x.Order.OrderedAt)
                .ToListAsync(cancellationToken);

            var fallbackStatus = await _unitOfWork.Repository<OrderItem, long>().AsQueryable()
                .Include(x => x.Product)
                .Include(x => x.Order)
                .Where(x => productSkus.Contains(x.Product.Sku)
                            && (x.Order.Status == OrderStatus.Pending
                                || x.Order.Status == OrderStatus.Approved
                                || x.Order.Status == OrderStatus.PreOrder))
                .OrderByDescending(x => x.Order.OrderedAt)
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                if (movementBySku.TryGetValue(item.ItemCode, out var movementRows) && movementRows.Any())
                {
                    item.Details = movementRows.Select(ToDetail)
                        .OrderByDescending(x => x.OccurredAt ?? x.ReceiveDate)
                        .ToList();
                    continue;
                }

                var legacyDetails = new List<StockReportDetailDto>();

                legacyDetails.AddRange(fallbackAdjustmentItems
                    .Where(x => x.Product.Sku == item.ItemCode)
                    .Select(x => new StockReportDetailDto
                    {
                        ShelfNumber = x.ShelfNumber,
                        PackList = x.PackList,
                        ReceiveDate = x.StockAdjustment.AdjustmentDate,
                        OccurredAt = x.StockAdjustment.AdjustmentDate,
                        Quantity = x.NewQuantity - x.OldQuantity,
                        MovementType = StockMovementType.Adjustment.ToString(),
                        MovementGroup = "Adjustment",
                        SourceDocumentType = "StockAdjustment",
                        ReferenceNo = x.StockAdjustmentId.ToString(),
                        WarehouseId = x.WarehouseId,
                        SupplierName = x.SupplierName
                    }));

                legacyDetails.AddRange(fallbackPurchaseReceives
                    .Where(x => x.Product.Sku == item.ItemCode)
                    .Select(x => new StockReportDetailDto
                    {
                        ShelfNumber = item.ShelfNumber,
                        PackList = x.PurchaseOrder.OrderNumber,
                        ReceiveDate = x.PurchaseOrder.OrderedAt,
                        OccurredAt = x.PurchaseOrder.OrderedAt,
                        Quantity = x.ReceivedQuantity,
                        MovementType = StockMovementType.PurchaseReceive.ToString(),
                        MovementGroup = "Purchase",
                        SourceDocumentType = "PurchaseOrder",
                        ReferenceNo = x.PurchaseOrder.OrderNumber
                    }));

                legacyDetails.AddRange(fallbackSales
                    .Where(x => x.Product.Sku == item.ItemCode)
                    .Select(x => new StockReportDetailDto
                    {
                        Quantity = -x.Quantity,
                        ReceiveDate = x.Order.ShippedAt ?? x.Order.OrderedAt,
                        OccurredAt = x.Order.ShippedAt ?? x.Order.OrderedAt,
                        MovementType = StockMovementType.Sale.ToString(),
                        MovementGroup = "Sale",
                        SourceDocumentType = "Order",
                        ReferenceNo = x.Order.OrderNumber,
                        WarehouseId = x.WarehouseId
                    }));

                legacyDetails.AddRange(fallbackStatus
                    .Where(x => x.Product.Sku == item.ItemCode)
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
                StockMovementType.PurchaseReceive => "Purchase",
                StockMovementType.Sale or StockMovementType.SaleRevert => "Sale",
                StockMovementType.Reserve or StockMovementType.Unreserve => "Status",
                _ => "Status"
            };
        }
    }
}
