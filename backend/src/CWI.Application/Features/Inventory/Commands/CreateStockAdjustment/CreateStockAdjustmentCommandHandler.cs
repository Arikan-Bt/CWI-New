using CWI.Application.DTOs.Inventory;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Products;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace CWI.Application.Features.Inventory.Commands.CreateStockAdjustment;

public record CreateStockAdjustmentCommand(CreateStockAdjustmentRequest Request) : IRequest<CreateStockAdjustmentResponse>;

public class CreateStockAdjustmentCommandHandler : IRequestHandler<CreateStockAdjustmentCommand, CreateStockAdjustmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockAdjustmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateStockAdjustmentResponse> Handle(CreateStockAdjustmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            using var stream = request.File.OpenReadStream();
            var excelData = stream.Query<StockAdjustmentExcelModel>().ToList();

            if (!excelData.Any())
            {
                return new CreateStockAdjustmentResponse
                {
                    Message = "The Excel file is empty or has an invalid format."
                };
            }

            var adjustment = new StockAdjustment
            {
                AdjustmentDate = request.AdjustmentDate,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            var adjustmentRepo = _unitOfWork.Repository<StockAdjustment, long>();
            await adjustmentRepo.AddAsync(adjustment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var processedCount = 0;
            var warnings = new List<StockAdjustmentWarningDto>();

            var productRepo = _unitOfWork.Repository<Product>();
            var inventoryRepo = _unitOfWork.Repository<InventoryItem, long>();
            var adjustmentItemRepo = _unitOfWork.Repository<StockAdjustmentItem, long>();
            var stockMovementRepo = _unitOfWork.Repository<StockMovement, long>();
            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();

            var normalizedSkuSet = excelData
                .Select(x => Normalize(x.ProductCode))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var products = await productRepo.AsQueryable()
                .Where(p => normalizedSkuSet.Contains(Normalize(p.Sku)))
                .ToListAsync(cancellationToken);
            var productBySku = products.ToDictionary(p => Normalize(p.Sku), p => p);

            var normalizedWarehouseSet = excelData
                .Where(x => !string.IsNullOrWhiteSpace(x.Warehouse))
                .Select(x => Normalize(x.Warehouse))
                .Distinct()
                .ToList();

            var warehouses = await warehouseRepo.AsQueryable()
                .Where(w => normalizedWarehouseSet.Contains(Normalize(w.Name)) || normalizedWarehouseSet.Contains(Normalize(w.Code)))
                .ToListAsync(cancellationToken);

            var warehouseByNameOrCode = new Dictionary<string, Warehouse>();
            foreach (var warehouse in warehouses)
            {
                var nameKey = Normalize(warehouse.Name);
                var codeKey = Normalize(warehouse.Code);

                if (!warehouseByNameOrCode.ContainsKey(nameKey))
                {
                    warehouseByNameOrCode[nameKey] = warehouse;
                }

                if (!warehouseByNameOrCode.ContainsKey(codeKey))
                {
                    warehouseByNameOrCode[codeKey] = warehouse;
                }
            }

            var defaultWarehouseId =
                await warehouseRepo.AsQueryable().Where(w => w.IsDefault).Select(w => (int?)w.Id).FirstOrDefaultAsync(cancellationToken)
                ?? await warehouseRepo.AsQueryable().Where(w => w.IsActive).Select(w => (int?)w.Id).FirstOrDefaultAsync(cancellationToken)
                ?? 1;

            for (var index = 0; index < excelData.Count; index++)
            {
                var item = excelData[index];
                var rowNo = index + 2;
                var normalizedSku = Normalize(item.ProductCode);

                if (string.IsNullOrWhiteSpace(normalizedSku) || !productBySku.TryGetValue(normalizedSku, out var product))
                {
                    warnings.Add(new StockAdjustmentWarningDto
                    {
                        Row = rowNo,
                        ProductCode = item.ProductCode ?? string.Empty,
                        Reason = "Product not found."
                    });
                    continue;
                }

                var warehouseId = defaultWarehouseId;
                if (!string.IsNullOrWhiteSpace(item.Warehouse))
                {
                    var warehouseKey = Normalize(item.Warehouse);
                    if (!warehouseByNameOrCode.TryGetValue(warehouseKey, out var warehouse))
                    {
                        warnings.Add(new StockAdjustmentWarningDto
                        {
                            Row = rowNo,
                            ProductCode = item.ProductCode ?? string.Empty,
                            Reason = $"Warehouse not found: {item.Warehouse}. Default warehouse was used."
                        });
                    }
                    else
                    {
                        warehouseId = warehouse.Id;
                    }
                }

                var inventoryItem = await inventoryRepo.AsQueryableTracking().FirstOrDefaultAsync(
                    i => i.ProductId == product.Id && i.WarehouseId == warehouseId,
                    cancellationToken);

                var oldOnHand = inventoryItem?.QuantityOnHand ?? 0;
                var oldReserved = inventoryItem?.QuantityReserved ?? 0;
                var newOnHand = item.Quantity;

                var adjustmentItem = new StockAdjustmentItem
                {
                    StockAdjustmentId = adjustment.Id,
                    ProductId = product.Id,
                    OldQuantity = oldOnHand,
                    NewQuantity = newOnHand,
                    Price = item.Price,
                    Currency = item.Currency,
                    ShelfNumber = item.ShelfNumber,
                    PackList = item.PackList,
                    ReceivingNumber = item.ReceivingNo,
                    WarehouseId = warehouseId,
                    SupplierName = item.Supplier
                };

                await adjustmentItemRepo.AddAsync(adjustmentItem, cancellationToken);

                if (inventoryItem == null)
                {
                    inventoryItem = new InventoryItem
                    {
                        WarehouseId = warehouseId,
                        ProductId = product.Id,
                        QuantityOnHand = newOnHand,
                        UpdatedAt = DateTime.UtcNow,
                        ShelfNumber = item.ShelfNumber
                    };
                    await inventoryRepo.AddAsync(inventoryItem, cancellationToken);
                }
                else
                {
                    inventoryItem.QuantityOnHand = newOnHand;
                    inventoryItem.ShelfNumber = item.ShelfNumber;
                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                    inventoryRepo.Update(inventoryItem);
                }

                await stockMovementRepo.AddAsync(new StockMovement
                {
                    ProductId = product.Id,
                    WarehouseId = warehouseId,
                    MovementType = StockMovementType.Adjustment,
                    QuantityDeltaOnHand = newOnHand - oldOnHand,
                    QuantityDeltaReserved = 0,
                    BeforeOnHand = oldOnHand,
                    AfterOnHand = newOnHand,
                    BeforeReserved = oldReserved,
                    AfterReserved = oldReserved,
                    SourceDocumentType = "StockAdjustment",
                    SourceDocumentId = adjustment.Id,
                    ReferenceNo = adjustment.Id.ToString(),
                    ShelfNumber = item.ShelfNumber,
                    PackList = item.PackList,
                    SupplierName = item.Supplier,
                    OccurredAt = adjustment.AdjustmentDate,
                    Description = request.Description
                }, cancellationToken);

                processedCount++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateStockAdjustmentResponse
            {
                Id = adjustment.Id,
                ProcessedItemsCount = processedCount,
                SkippedItemsCount = excelData.Count - processedCount,
                Warnings = warnings,
                Message = $"{processedCount} products were adjusted successfully."
            };
        }
        catch (Exception ex)
        {
            return new CreateStockAdjustmentResponse
            {
                Message = $"An error occurred during processing: {ex.Message}"
            };
        }
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}
