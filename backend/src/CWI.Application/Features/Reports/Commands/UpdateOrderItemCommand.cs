using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Commands;

public class UpdateOrderItemCommand : IRequest<bool>
{
    public UpdateOrderItemRequest Request { get; set; } = null!;

    public class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateOrderItemCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var orderRepo = _unitOfWork.Repository<Order, long>();
            var productRepo = _unitOfWork.Repository<Product, int>();
            var orderItemRepo = _unitOfWork.Repository<OrderItem, long>();

            var order = await orderRepo.AsQueryable()
                .Include(o => o.Items)
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) return false;

            var existingItem = order.Items.FirstOrDefault(i => i.Product != null && i.Product.Sku == request.ProductCode);

            if (existingItem != null)
            {
                // Mevcut kalemi güncelle
                existingItem.Quantity = request.Qty;
                existingItem.UnitPrice = request.Amount;
                existingItem.LineTotal = request.Qty * request.Amount;
                existingItem.NetTotal = existingItem.LineTotal; // Basit tutuyoruz
                existingItem.Notes = request.Notes;
            }
            else
            {
                // Yeni kalem ekle
                var product = await productRepo.AsQueryable()
                    .FirstOrDefaultAsync(p => p.Sku == request.ProductCode, cancellationToken);

                if (product == null) return false;

                var newItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = request.Qty,
                    UnitPrice = request.Amount,
                    LineTotal = request.Qty * request.Amount,
                    NetTotal = request.Qty * request.Amount,
                    Notes = request.Notes,
                    WarehouseId = 1 // Varsayılan depo ID
                };

                order.Items.Add(newItem);
            }

            // Sipariş toplamlarını yeniden hesapla
            RecalculateOrderTotals(order);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private void RecalculateOrderTotals(Order order)
        {
            order.TotalQuantity = order.Items.Sum(i => i.Quantity);
            order.SubTotal = order.Items.Sum(i => i.LineTotal);
            
            decimal discountPercent = order.ShippingInfo?.AdditionalDiscount ?? 0;
            order.TotalDiscount = order.SubTotal * (discountPercent / 100);
            order.GrandTotal = order.SubTotal - order.TotalDiscount;
            order.TaxableAmount = order.GrandTotal; // Basit vergi hesabı
        }
    }
}
