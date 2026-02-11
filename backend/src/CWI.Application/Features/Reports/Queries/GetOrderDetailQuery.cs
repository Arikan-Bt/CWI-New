using System.Linq;
using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public class GetOrderDetailQuery : IRequest<OrderDetailResponse>
{
    public long OrderId { get; set; }
    public string? Brand { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetOrderDetailQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderDetailResponse> Handle(GetOrderDetailQuery query, CancellationToken cancellationToken)
        {
            var orderItemRepo = _unitOfWork.Repository<OrderItem, long>();
            
            var queryable = orderItemRepo.AsQueryable()
                .Include(i => i.Product)
                    .ThenInclude(p => p.Brand)
                .Where(i => i.OrderId == query.OrderId)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(query.Brand))
            {
                queryable = queryable.Where(i => i.Product != null && i.Product.Brand != null && i.Product.Brand.Name == query.Brand);
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .OrderBy(i => i.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var data = items.Select(i => new OrderDetailDto
            {
                Id = i.Id,
                ProductCode = i.Product != null ? i.Product.Sku : string.Empty,
                ProductName = i.Product != null ? i.Product.Name : string.Empty,
                Picture = i.Product != null ? $"https://cdn.arikantime.com/ProductImages/{i.Product.Sku}.jpg" : null,
                Qty = i.Quantity,
                Amount = i.UnitPrice,
                Total = i.LineTotal,
                Attributes = i.Product != null ? i.Product.Attributes : null
            }).ToList();

            return new OrderDetailResponse
            {
                Data = data,
                TotalCount = totalCount
            };
        }
    }
}
