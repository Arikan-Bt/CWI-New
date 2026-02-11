using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

/// <summary>
/// Satın alma siparişi fatura raporunu getiren sorgu
/// </summary>
public class GetPurchaseOrderInvoiceReportQuery : IRequest<PurchaseOrderInvoiceReportResponse>
{
    public PurchaseOrderInvoiceReportRequest Request { get; set; } = null!;

    public class GetPurchaseOrderInvoiceReportQueryHandler : IRequestHandler<GetPurchaseOrderInvoiceReportQuery, PurchaseOrderInvoiceReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPurchaseOrderInvoiceReportQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PurchaseOrderInvoiceReportResponse> Handle(GetPurchaseOrderInvoiceReportQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var orderRepo = _unitOfWork.Repository<PurchaseOrder, long>();
            
            var queryable = orderRepo.AsQueryable()
                .Include(o => o.Items)
                .AsNoTracking();

            // Tarih filtreleri
            if (request.StartDate.HasValue)
            {
                queryable = queryable.Where(o => o.OrderedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                queryable = queryable.Where(o => o.OrderedAt <= request.EndDate.Value);
            }

            // Arama filtresi
            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var search = request.SearchQuery.ToLower();
                queryable = queryable.Where(o => 
                    o.OrderNumber.ToLower().Contains(search) || 
                    (o.ExternalReference != null && o.ExternalReference.ToLower().Contains(search)));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            // Sıralama
            if (!string.IsNullOrEmpty(request.SortField))
            {
                bool isAsc = request.SortOrder == 1;
                switch (request.SortField)
                {
                    case "invoiceDate":
                        queryable = isAsc ? queryable.OrderBy(o => o.OrderedAt) : queryable.OrderByDescending(o => o.OrderedAt);
                        break;
                    case "orderRefNo":
                        queryable = isAsc ? queryable.OrderBy(o => o.OrderNumber) : queryable.OrderByDescending(o => o.OrderNumber);
                        break;
                    case "invoiceRefNum":
                        queryable = isAsc ? queryable.OrderBy(o => o.ExternalReference) : queryable.OrderByDescending(o => o.ExternalReference);
                        break;
                    case "invoiceQty":
                         queryable = isAsc ? queryable.OrderBy(o => o.Items.Sum(i => i.ReceivedQuantity)) : queryable.OrderByDescending(o => o.Items.Sum(i => i.ReceivedQuantity));
                        break;
                    case "invoiceAmount":
                         queryable = isAsc ? queryable.OrderBy(o => o.Items.Sum(i => i.ReceivedQuantity * i.UnitPrice)) : queryable.OrderByDescending(o => o.Items.Sum(i => i.ReceivedQuantity * i.UnitPrice));
                        break;
                    case "orderQty":
                        queryable = isAsc ? queryable.OrderBy(o => o.TotalQuantity) : queryable.OrderByDescending(o => o.TotalQuantity);
                        break;
                    case "orderAmount":
                        queryable = isAsc ? queryable.OrderBy(o => o.TotalAmount) : queryable.OrderByDescending(o => o.TotalAmount);
                        break;
                    default:
                        queryable = queryable.OrderByDescending(o => o.OrderedAt);
                        break;
                }
            }
            else
            {
                queryable = queryable.OrderByDescending(o => o.OrderedAt);
            }

            var projectedData = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new PurchaseOrderInvoiceReportDto
                {
                    Id = o.Id,
                    InvoiceDate = o.OrderedAt, // Sipariş tarihi veya varsa son fatura tarihi
                    InvoiceRefNum = o.ExternalReference ?? "N/A",
                    OrderRefNo = o.OrderNumber,
                    // Teslim alınan miktarların toplamı 'InvoiceQty' olarak kabul ediliyor
                    InvoiceQty = o.Items.Sum(i => i.ReceivedQuantity),
                    // Ortalama birim fiyat üzerinden basit bir hesaplama (Gerçek fatura tablosu yoksa)
                    InvoiceAmount = o.Items.Sum(i => i.ReceivedQuantity * i.UnitPrice),
                    OrderQty = o.TotalQuantity,
                    OrderAmount = o.TotalAmount
                })
                .ToListAsync(cancellationToken);

            return new PurchaseOrderInvoiceReportResponse
            {
                Data = projectedData,
                TotalCount = totalCount
            };
        }
    }
}
