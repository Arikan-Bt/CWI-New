using CWI.Application.DTOs.Purchasing;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Purchasing.Queries.GetPurchaseOrderDetails;

public class GetPurchaseOrderDetailsQuery : IRequest<PurchaseOrderDetailDto>
{
    public long Id { get; set; }

    public class GetPurchaseOrderDetailsQueryHandler : IRequestHandler<GetPurchaseOrderDetailsQuery, PurchaseOrderDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPurchaseOrderDetailsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PurchaseOrderDetailDto> Handle(GetPurchaseOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<PurchaseOrder, long>();
            var order = await repo.AsQueryable()
                .Include(x => x.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (order == null) return new PurchaseOrderDetailDto();

            return new PurchaseOrderDetailDto
            {
                Id = order.Id,
                OrderRefNo = order.OrderNumber,
                Date = order.OrderedAt,
                Items = order.Items.Select(x => new PurchaseOrderItemDto
                {
                    Id = x.Id,
                    ProductCode = x.ProductCode,
                    ProductName = x.ProductName,
                    OrderQty = x.Quantity,
                    OrderUnitPrice = x.UnitPrice,
                    OrderAmount = x.LineTotal,
                    Receive = x.ReceivedQuantity,
                    Balance = x.Quantity - x.ReceivedQuantity,
                    InvoiceQty = 0, // Yeni fatura için başlangıçta 0
                    InvoiceUnitPrice = x.UnitPrice
                }).ToList()
            };
        }
    }
}
