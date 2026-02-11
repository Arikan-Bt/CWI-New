using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Commands;

public class RemoveOrderItemCommand : IRequest<bool>
{
    public RemoveOrderItemRequest Request { get; set; } = null!;

    public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveOrderItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveOrderItemCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var orderRepo = _unitOfWork.Repository<Order, long>();

            var order = await orderRepo.AsQueryable()
                .Include(o => o.Items)
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) return false;

            var itemToRemove = order.Items.FirstOrDefault(i => i.Product != null && i.Product.Sku == request.ProductCode);

            if (itemToRemove != null)
            {
                order.Items.Remove(itemToRemove);
                
                // Sipariş toplamlarını yeniden hesapla
                RecalculateOrderTotals(order);

                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
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
