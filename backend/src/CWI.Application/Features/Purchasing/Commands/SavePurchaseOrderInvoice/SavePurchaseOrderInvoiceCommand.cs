using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Entities.Purchasing;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Purchasing.Commands.SavePurchaseOrderInvoice;

public class SavePurchaseOrderInvoiceCommand : IRequest<bool>
{
    public long OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public IFormFile? InvoiceFile { get; set; }
    public List<InvoiceLineItemRequest> Lines { get; set; } = new();

    public class InvoiceLineItemRequest
    {
        public long ItemId { get; set; }
        public int InvoiceQty { get; set; }
        public decimal InvoiceUnitPrice { get; set; }
        public int? WarehouseId { get; set; }
        public string? ShelfNumber { get; set; }
        public string? PackList { get; set; }
        public DateTime? ReceivingDate { get; set; }
    }

    public class SavePurchaseOrderInvoiceCommandHandler : IRequestHandler<SavePurchaseOrderInvoiceCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] AllowedFileExtensions = [".pdf", ".jpg", ".jpeg", ".png"];

        public SavePurchaseOrderInvoiceCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task<bool> Handle(SavePurchaseOrderInvoiceCommand request, CancellationToken cancellationToken)
        {
            var itemRepo = _unitOfWork.Repository<PurchaseOrderItem, long>();
            var inventoryRepo = _unitOfWork.Repository<InventoryItem, long>();
            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();
            var stockAdjustmentRepo = _unitOfWork.Repository<StockAdjustment, long>();
            var stockAdjustmentItemRepo = _unitOfWork.Repository<StockAdjustmentItem, long>();
            var stockMovementRepo = _unitOfWork.Repository<StockMovement, long>();
            var defaultWarehouseId =
                await warehouseRepo.AsQueryable().Where(w => w.IsDefault).Select(w => (int?)w.Id).FirstOrDefaultAsync(cancellationToken)
                ?? await warehouseRepo.AsQueryable().Where(w => w.IsActive).Select(w => (int?)w.Id).FirstOrDefaultAsync(cancellationToken);

            var orderRepo = _unitOfWork.Repository<PurchaseOrder, long>();
            var order = await orderRepo.AsQueryable()
                .Include(o => o.Supplier)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            decimal totalInvoiceAmount = 0;

            foreach (var line in request.Lines)
            {
                var item = await itemRepo.GetByIdAsync(line.ItemId);
                if (item != null && line.InvoiceQty > 0)
                {
                    item.ReceivedQuantity += line.InvoiceQty;
                    itemRepo.Update(item);

                    totalInvoiceAmount += line.InvoiceQty * line.InvoiceUnitPrice;

                    var targetWarehouseId = line.WarehouseId ?? defaultWarehouseId;
                    if (targetWarehouseId.HasValue)
                    {
                        var inventoryItem = await inventoryRepo.FirstOrDefaultAsync(
                            x => x.ProductId == item.ProductId && x.WarehouseId == targetWarehouseId.Value,
                            cancellationToken);
                        var oldQuantity = inventoryItem?.QuantityOnHand ?? 0;

                        if (inventoryItem == null)
                        {
                            inventoryItem = new InventoryItem
                            {
                                ProductId = item.ProductId,
                                WarehouseId = targetWarehouseId.Value,
                                QuantityOnHand = 0,
                                ShelfNumber = line.ShelfNumber
                            };
                            await inventoryRepo.AddAsync(inventoryItem, cancellationToken);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(line.ShelfNumber))
                            {
                                inventoryItem.ShelfNumber = line.ShelfNumber;
                            }
                            inventoryRepo.Update(inventoryItem);
                        }

                        inventoryItem.QuantityOnHand += line.InvoiceQty;
                        var afterQuantity = inventoryItem.QuantityOnHand;

                        // Keep a receive trail for Stock Report row expansion (Shelf/Pack/Receive Date).
                        var receiveLog = new StockAdjustment
                        {
                            AdjustmentDate = line.ReceivingDate ?? request.InvoiceDate,
                            Description = $"Purchase Receive - Invoice: {request.InvoiceNumber}",
                            CreatedAt = DateTime.UtcNow
                        };

                        var receiveLogItem = new StockAdjustmentItem
                        {
                            StockAdjustment = receiveLog,
                            ProductId = item.ProductId,
                            OldQuantity = oldQuantity,
                            NewQuantity = inventoryItem.QuantityOnHand,
                            ShelfNumber = line.ShelfNumber ?? inventoryItem.ShelfNumber,
                            PackList = line.PackList,
                            ReceivingNumber = request.InvoiceNumber,
                            WarehouseId = targetWarehouseId.Value,
                            SupplierName = order?.Supplier?.Name,
                            Price = line.InvoiceUnitPrice,
                            Currency = "USD"
                        };

                        await stockAdjustmentRepo.AddAsync(receiveLog, cancellationToken);
                        await stockAdjustmentItemRepo.AddAsync(receiveLogItem, cancellationToken);

                        await stockMovementRepo.AddAsync(new StockMovement
                        {
                            ProductId = item.ProductId,
                            WarehouseId = targetWarehouseId.Value,
                            MovementType = StockMovementType.PurchaseReceive,
                            QuantityDeltaOnHand = line.InvoiceQty,
                            QuantityDeltaReserved = 0,
                            BeforeOnHand = oldQuantity,
                            AfterOnHand = afterQuantity,
                            BeforeReserved = inventoryItem.QuantityReserved,
                            AfterReserved = inventoryItem.QuantityReserved,
                            SourceDocumentType = "PurchaseOrderInvoice",
                            SourceDocumentId = request.OrderId,
                            ReferenceNo = request.InvoiceNumber,
                            ShelfNumber = line.ShelfNumber ?? inventoryItem.ShelfNumber,
                            PackList = line.PackList,
                            SupplierName = order?.Supplier?.Name,
                            OccurredAt = line.ReceivingDate ?? request.InvoiceDate,
                            Description = $"Purchase Receive - Invoice: {request.InvoiceNumber}"
                        }, cancellationToken);
                    }
                }
            }

            string? invoiceFilePath = null;
            if (request.InvoiceFile != null && request.InvoiceFile.Length > 0)
            {
                var extension = Path.GetExtension(request.InvoiceFile.FileName).ToLowerInvariant();
                if (!AllowedFileExtensions.Contains(extension))
                {
                    throw new InvalidOperationException(
                        $"Invalid file format. Allowed extensions: {string.Join(", ", AllowedFileExtensions)}");
                }

                var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "invoices");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                invoiceFilePath = Path.Combine("uploads", "invoices", fileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await request.InvoiceFile.CopyToAsync(stream, cancellationToken);
            }

            if (order?.SupplierId != null && totalInvoiceAmount > 0)
            {
                var currencyRepo = _unitOfWork.Repository<Currency, int>();
                var currency = await currencyRepo.FirstOrDefaultAsync(c => c.Code == "USD", cancellationToken);

                var vendorInvoiceRepo = _unitOfWork.Repository<VendorInvoice, int>();

                var existingInvoice = await vendorInvoiceRepo.FirstOrDefaultAsync(
                    v => v.InvoiceNumber == request.InvoiceNumber && v.VendorId == order.SupplierId.Value,
                    cancellationToken);

                if (existingInvoice == null)
                {
                    var vendorInvoice = new VendorInvoice
                    {
                        VendorId = order.SupplierId.Value,
                        InvoiceNumber = request.InvoiceNumber,
                        InvoicedAt = request.InvoiceDate,
                        TotalAmount = totalInvoiceAmount,
                        CurrencyId = currency?.Id ?? 1,
                        Description = $"Purchase Order: {order.OrderNumber}",
                        FilePath = invoiceFilePath,
                        IsPaid = false,
                        PaidAmount = 0
                    };
                    await vendorInvoiceRepo.AddAsync(vendorInvoice, cancellationToken);
                }
                else
                {
                    existingInvoice.InvoicedAt = request.InvoiceDate;
                    existingInvoice.TotalAmount = totalInvoiceAmount;
                    existingInvoice.CurrencyId = currency?.Id ?? existingInvoice.CurrencyId;
                    existingInvoice.Description = $"Purchase Order: {order.OrderNumber}";
                    if (!string.IsNullOrWhiteSpace(invoiceFilePath))
                    {
                        existingInvoice.FilePath = invoiceFilePath;
                    }
                    vendorInvoiceRepo.Update(existingInvoice);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
