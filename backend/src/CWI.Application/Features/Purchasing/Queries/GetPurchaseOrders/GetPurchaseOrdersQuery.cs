using CWI.Application.DTOs.Purchasing;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Purchasing.Queries.GetPurchaseOrders;

public class GetPurchaseOrdersQuery : IRequest<PurchaseOrderListResponse>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public int SortOrder { get; set; }
    public string? FilterOrderRefNo { get; set; }
    public string? FilterCustomerSvc { get; set; }

    public class GetPurchaseOrdersQueryHandler : IRequestHandler<GetPurchaseOrdersQuery, PurchaseOrderListResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPurchaseOrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PurchaseOrderListResponse> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<PurchaseOrder, long>();
            var query = repo.AsQueryable().AsNoTracking();

            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.OrderedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.OrderedAt <= request.EndDate.Value);
            }

            // Filters
            if (!string.IsNullOrEmpty(request.FilterOrderRefNo))
            {
                var filter = request.FilterOrderRefNo.ToLower();
                query = query.Where(x => x.OrderNumber.ToLower().Contains(filter));
            }

            if (!string.IsNullOrEmpty(request.FilterCustomerSvc))
            {
                var filter = request.FilterCustomerSvc.ToLower();
                query = query.Where(x => x.SupplierName != null && x.SupplierName.ToLower().Contains(filter));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Sorting
            if (!string.IsNullOrEmpty(request.SortField))
            {
                bool isAsc = request.SortOrder == 1;
                switch (request.SortField)
                {
                    case "orderRefNo":
                        query = isAsc ? query.OrderBy(x => x.OrderNumber) : query.OrderByDescending(x => x.OrderNumber);
                        break;
                    case "customerSvc":
                         query = isAsc ? query.OrderBy(x => x.SupplierName) : query.OrderByDescending(x => x.SupplierName);
                        break;
                    case "date":
                         query = isAsc ? query.OrderBy(x => x.OrderedAt) : query.OrderByDescending(x => x.OrderedAt);
                        break;
                    case "qty":
                         query = isAsc ? query.OrderBy(x => x.TotalQuantity) : query.OrderByDescending(x => x.TotalQuantity);
                        break;
                    case "amount":
                         query = isAsc ? query.OrderBy(x => x.TotalAmount) : query.OrderByDescending(x => x.TotalAmount);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.OrderedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.OrderedAt);
            }

            var orders = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new PurchaseOrderDto
                {
                    Id = x.Id,
                    Date = x.OrderedAt,
                    OrderRefNo = x.OrderNumber,
                    DocumentNumber = x.DocumentNumber.ToString(),
                    CustomerSvc = x.SupplierName ?? string.Empty,
                    Qty = x.TotalQuantity,
                    Amount = x.TotalAmount,
                    Status = x.IsReceived ? "Inactive" : "Active" 
                })
                .ToListAsync(cancellationToken);

            return new PurchaseOrderListResponse { Data = orders, TotalCount = totalCount };
        }
    }
}
