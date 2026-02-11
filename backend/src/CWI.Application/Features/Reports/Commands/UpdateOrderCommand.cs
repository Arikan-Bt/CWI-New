using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Inventory;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Commands;

public class UpdateOrderCommand : IRequest<bool>
{
    public UpdateOrderRequest Request { get; set; } = null!;

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var orderRepo = _unitOfWork.Repository<Order, long>();
            var previousStatus = OrderStatus.Draft;
            var targetStatus = OrderStatus.Draft;

            var order = await orderRepo.AsQueryableTracking()
                .Include(o => o.ShippingInfo)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) return false;
            previousStatus = order.Status;
            targetStatus = order.Status;

            // Header bilgilerini güncelle
            order.Notes = request.OrderDescription;
            if (TryParseOrderStatus(request.Status, out var status))
            {
                targetStatus = status;
                order.Status = status;
            }
            
            order.ShippedAt = request.RequestedShipmentDate;

            // ShippingInfo güncelle veya oluştur
            if (order.ShippingInfo == null)
            {
                order.ShippingInfo = new OrderShippingInfo
                {
                    OrderId = order.Id,
                    PaymentMethod = request.PaymentType,
                    ShipmentTerms = request.ShipmentMethod,
                    AdditionalDiscount = request.DiscountPercent
                };
            }
            else
            {
                order.ShippingInfo.PaymentMethod = request.PaymentType;
                order.ShippingInfo.ShipmentTerms = request.ShipmentMethod;
                order.ShippingInfo.AdditionalDiscount = request.DiscountPercent;
            }

            // Silinecek ürünler varsa sil
            if (request.RemovedProductCodes != null && request.RemovedProductCodes.Any())
            {
                var itemsToRemove = order.Items
                    .Where(i => i.Product != null && request.RemovedProductCodes.Contains(i.Product.Sku))
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    order.Items.Remove(item);
                }
                
                // Toplamları yeniden hesapla
                RecalculateOrderTotals(order);
            }

            // Warehouse seçimlerini uygula
            if (request.WarehouseSelections != null && request.WarehouseSelections.Any())
            {
                foreach (var selection in request.WarehouseSelections)
                {
                    var item = order.Items.FirstOrDefault(i => i.Product != null && i.Product.Sku == selection.ProductCode);
                    if (item != null)
                    {
                        item.WarehouseId = selection.WarehouseId;
                    }
                }
            }

            // Durum geçişine göre stokları senkronize et.
            // Canceled durumunda daha önce düşülen rezerv/on-hand miktarları geri alınır.
            if (previousStatus != targetStatus)
            {
                await ApplyStockTransitionAsync(order, previousStatus, targetStatus, cancellationToken);
            }

            order.IsCanceled = targetStatus == OrderStatus.Canceled;

            // CustomerTransaction kaydı oluştur (eğer henüz yoksa)
            // Bu kayıt sayesinde Payment Received modalında Reference Code dropdown'unda siparişler görünür
            RecalculateOrderTotals(order);
            await CreateCustomerTransactionIfNotExists(order, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static bool TryParseOrderStatus(string? rawStatus, out OrderStatus status)
        {
            status = default;
            if (string.IsNullOrWhiteSpace(rawStatus))
            {
                return false;
            }

            if (Enum.TryParse<OrderStatus>(rawStatus, true, out status))
            {
                return true;
            }

            // Support UI labels like "Pre Order" and "Packed & Waiting Shipment"
            static string Normalize(string value) =>
                value.ToLowerInvariant().Replace("&", "and").Replace(" ", string.Empty);

            var normalized = Normalize(rawStatus);

            foreach (var enumName in Enum.GetNames(typeof(OrderStatus)))
            {
                if (Normalize(enumName) == normalized)
                {
                    status = Enum.Parse<OrderStatus>(enumName, true);
                    return true;
                }
            }

            return false;
        }

        private async Task ApplyStockTransitionAsync(
            Order order,
            OrderStatus fromStatus,
            OrderStatus toStatus,
            CancellationToken cancellationToken)
        {
            if (order.Items.Count == 0) return;

            if (!AffectsStock(fromStatus) && !AffectsStock(toStatus)) return;

            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();
            var defaultWarehouseId =
                await warehouseRepo.AsQueryable().Where(w => w.IsDefault).Select(w => (int?)w.Id).FirstOrDefaultAsync(cancellationToken)
                ?? await warehouseRepo.AsQueryable().Where(w => w.IsActive).Select(w => (int?)w.Id).FirstOrDefaultAsync(cancellationToken)
                ?? 1;

            var groupedLines = order.Items
                .GroupBy(i => new
                {
                    i.ProductId,
                    WarehouseId = i.WarehouseId > 0 ? i.WarehouseId : defaultWarehouseId
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.WarehouseId,
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToList();

            var productIds = groupedLines.Select(x => x.ProductId).Distinct().ToList();
            var warehouseIds = groupedLines.Select(x => x.WarehouseId).Distinct().ToList();

            var inventoryRepo = _unitOfWork.Repository<InventoryItem, long>();
            var stockMovementRepo = _unitOfWork.Repository<StockMovement, long>();
            var inventoryItems = await inventoryRepo.AsQueryableTracking()
                .Where(i => productIds.Contains(i.ProductId) && warehouseIds.Contains(i.WarehouseId))
                .ToListAsync(cancellationToken);

            foreach (var line in groupedLines)
            {
                var inventory = inventoryItems.FirstOrDefault(i =>
                    i.ProductId == line.ProductId && i.WarehouseId == line.WarehouseId);

                if (inventory == null)
                {
                    inventory = new InventoryItem
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        QuantityOnHand = 0,
                        QuantityReserved = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await inventoryRepo.AddAsync(inventory, cancellationToken);
                    inventoryItems.Add(inventory);
                }

                // 1) Eski statünün stok etkisini geri al
                if (ConsumesOnHand(fromStatus))
                {
                    var beforeOnHand = inventory.QuantityOnHand;
                    var beforeReserved = inventory.QuantityReserved;
                    inventory.QuantityOnHand += line.Qty;
                    await stockMovementRepo.AddAsync(new StockMovement
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        MovementType = StockMovementType.SaleRevert,
                        QuantityDeltaOnHand = line.Qty,
                        QuantityDeltaReserved = 0,
                        BeforeOnHand = beforeOnHand,
                        AfterOnHand = inventory.QuantityOnHand,
                        BeforeReserved = beforeReserved,
                        AfterReserved = inventory.QuantityReserved,
                        SourceDocumentType = "OrderStatusTransition",
                        SourceDocumentId = order.Id,
                        ReferenceNo = order.OrderNumber,
                        OccurredAt = DateTime.UtcNow,
                        Description = $"Order status transition {fromStatus} -> {toStatus}"
                    }, cancellationToken);
                }

                if (ReservesStock(fromStatus))
                {
                    var beforeOnHand = inventory.QuantityOnHand;
                    var beforeReserved = inventory.QuantityReserved;
                    inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - line.Qty);
                    await stockMovementRepo.AddAsync(new StockMovement
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        MovementType = StockMovementType.Unreserve,
                        QuantityDeltaOnHand = 0,
                        QuantityDeltaReserved = inventory.QuantityReserved - beforeReserved,
                        BeforeOnHand = beforeOnHand,
                        AfterOnHand = inventory.QuantityOnHand,
                        BeforeReserved = beforeReserved,
                        AfterReserved = inventory.QuantityReserved,
                        SourceDocumentType = "OrderStatusTransition",
                        SourceDocumentId = order.Id,
                        ReferenceNo = order.OrderNumber,
                        OccurredAt = DateTime.UtcNow,
                        Description = $"Order status transition {fromStatus} -> {toStatus}"
                    }, cancellationToken);
                }

                // 2) Yeni statünün stok etkisini uygula
                if (ConsumesOnHand(toStatus))
                {
                    var beforeOnHand = inventory.QuantityOnHand;
                    var beforeReserved = inventory.QuantityReserved;
                    inventory.QuantityOnHand -= line.Qty;
                    await stockMovementRepo.AddAsync(new StockMovement
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        MovementType = StockMovementType.Sale,
                        QuantityDeltaOnHand = -line.Qty,
                        QuantityDeltaReserved = 0,
                        BeforeOnHand = beforeOnHand,
                        AfterOnHand = inventory.QuantityOnHand,
                        BeforeReserved = beforeReserved,
                        AfterReserved = inventory.QuantityReserved,
                        SourceDocumentType = "OrderStatusTransition",
                        SourceDocumentId = order.Id,
                        ReferenceNo = order.OrderNumber,
                        OccurredAt = DateTime.UtcNow,
                        Description = $"Order status transition {fromStatus} -> {toStatus}"
                    }, cancellationToken);
                }

                if (ReservesStock(toStatus))
                {
                    var beforeOnHand = inventory.QuantityOnHand;
                    var beforeReserved = inventory.QuantityReserved;
                    inventory.QuantityReserved += line.Qty;
                    await stockMovementRepo.AddAsync(new StockMovement
                    {
                        ProductId = line.ProductId,
                        WarehouseId = line.WarehouseId,
                        MovementType = StockMovementType.Reserve,
                        QuantityDeltaOnHand = 0,
                        QuantityDeltaReserved = line.Qty,
                        BeforeOnHand = beforeOnHand,
                        AfterOnHand = inventory.QuantityOnHand,
                        BeforeReserved = beforeReserved,
                        AfterReserved = inventory.QuantityReserved,
                        SourceDocumentType = "OrderStatusTransition",
                        SourceDocumentId = order.Id,
                        ReferenceNo = order.OrderNumber,
                        OccurredAt = DateTime.UtcNow,
                        Description = $"Order status transition {fromStatus} -> {toStatus}"
                    }, cancellationToken);
                }

                inventory.UpdatedAt = DateTime.UtcNow;
                inventoryRepo.Update(inventory);
            }
        }

        private static bool AffectsStock(OrderStatus status) =>
            ConsumesOnHand(status) || ReservesStock(status);

        private static bool ConsumesOnHand(OrderStatus status) =>
            status == OrderStatus.Shipped;

        private static bool ReservesStock(OrderStatus status) =>
            status == OrderStatus.Pending
            || status == OrderStatus.PreOrder
            || status == OrderStatus.PackedAndWaitingShipment
            || status == OrderStatus.Approved;

        /// <summary>
        /// Sipariş için CustomerTransaction (Debit) kaydı oluşturur.
        /// Bu kayıt müşterinin cari hareketlerinde borç olarak görünür.
        /// Payment Received modalında Reference Code seçimi için kullanılır.
        /// </summary>
        private async Task CreateCustomerTransactionIfNotExists(Order order, CancellationToken cancellationToken)
        {
            // Müşteri atanmamışsa veya sipariş tutarı 0 ise işlem yapma
            if (order.CustomerId == 0 || order.GrandTotal <= 0) return;

            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
            
            // Aynı sipariş için daha önce transaction oluşturulmuş mu kontrol et
            var existingTransaction = await transactionRepo.FirstOrDefaultAsync(
                t => t.ReferenceNumber == order.OrderNumber && t.CustomerId == order.CustomerId,
                cancellationToken);
            
            // Eğer zaten varsa güncelle (tutar değişmiş olabilir)
            if (existingTransaction != null)
            {
                existingTransaction.DebitAmount = order.GrandTotal;
                existingTransaction.Balance = order.GrandTotal;
                existingTransaction.TransactionDate = order.ShippedAt ?? order.CreatedAt;
                transactionRepo.Update(existingTransaction);
                return;
            }

            // Yeni CustomerTransaction oluştur (Debit/Borç olarak)
            var transaction = new CustomerTransaction
            {
                CustomerId = order.CustomerId,
                TransactionType = TransactionType.Invoice,
                TransactionDate = order.ShippedAt ?? order.CreatedAt,
                ReferenceNumber = order.OrderNumber,
                Description = $"Sales Order - {order.Customer?.Name ?? "Customer"}",
                DocumentType = "Sales Invoice",
                ApplicationReference = order.Id.ToString(),
                DebitAmount = order.GrandTotal,
                CreditAmount = 0,
                Balance = order.GrandTotal,
                CreatedAt = DateTime.UtcNow
            };

            await transactionRepo.AddAsync(transaction, cancellationToken);
        }

        private void RecalculateOrderTotals(Order order)
        {
            order.TotalQuantity = order.Items.Sum(i => i.Quantity);
            order.SubTotal = order.Items.Sum(i => i.LineTotal);
            
            decimal discountPercent = order.ShippingInfo?.AdditionalDiscount ?? 0;
            order.TotalDiscount = order.SubTotal * (discountPercent / 100);
            order.GrandTotal = order.SubTotal - order.TotalDiscount;
            order.TaxableAmount = order.GrandTotal;
        }
    }
}

