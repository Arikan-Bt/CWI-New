using CWI.Application.DTOs.Purchasing;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Purchasing.Queries.GetVendorBalanceReport;

public class GetVendorBalanceReportQuery : IRequest<VendorBalanceReportResponse>
{
    public VendorBalanceReportRequest Request { get; set; } = null!;

    public class GetVendorBalanceReportQueryHandler : IRequestHandler<GetVendorBalanceReportQuery, VendorBalanceReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetVendorBalanceReportQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VendorBalanceReportResponse> Handle(GetVendorBalanceReportQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var invoiceRepo = _unitOfWork.Repository<VendorInvoice, int>();
            var paymentRepo = _unitOfWork.Repository<VendorPayment, long>();

            var queryable = invoiceRepo.AsQueryable()
                .Include(i => i.Vendor)
                .Include(i => i.Currency)
                .Include(i => i.Payments)
                    .ThenInclude(p => p.Currency)
                .AsNoTracking();

            if (request.StartDate.HasValue)
            {
                queryable = queryable.Where(i => i.InvoicedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                queryable = queryable.Where(i => i.InvoicedAt <= request.EndDate.Value);
            }

            // Filters
            if (!string.IsNullOrEmpty(request.FilterCurrAccCode))
            {
                var filter = request.FilterCurrAccCode.ToLower();
                queryable = queryable.Where(i => i.Vendor != null && i.Vendor.Code.ToLower().Contains(filter));
            }

            if (!string.IsNullOrEmpty(request.FilterCurrAccDescription))
            {
                var filter = request.FilterCurrAccDescription.ToLower();
                queryable = queryable.Where(i => i.Vendor != null && i.Vendor.Name.ToLower().Contains(filter));
            }

            if (!string.IsNullOrEmpty(request.FilterInvoiceNo))
            {
                var filter = request.FilterInvoiceNo.ToLower();
                queryable = queryable.Where(i => i.InvoiceNumber.ToLower().Contains(filter));
            }

            if (!string.IsNullOrEmpty(request.FilterDescription))
            {
                var filter = request.FilterDescription.ToLower();
                queryable = queryable.Where(i => i.Description != null && i.Description.ToLower().Contains(filter));
            }

            var totalCount = await queryable.CountAsync(cancellationToken);

            // Sorting
            if (!string.IsNullOrEmpty(request.SortField))
            {
                bool isAsc = request.SortOrder == 1;
                switch (request.SortField)
                {
                    case "currAccCode":
                        queryable = isAsc ? queryable.OrderBy(i => i.Vendor.Code) : queryable.OrderByDescending(i => i.Vendor.Code);
                        break;
                    case "currAccDescription":
                         queryable = isAsc ? queryable.OrderBy(i => i.Vendor.Name) : queryable.OrderByDescending(i => i.Vendor.Name);
                        break;
                    case "invoiceNo":
                         queryable = isAsc ? queryable.OrderBy(i => i.InvoiceNumber) : queryable.OrderByDescending(i => i.InvoiceNumber);
                        break;
                    case "invoiceDate":
                         queryable = isAsc ? queryable.OrderBy(i => i.InvoicedAt) : queryable.OrderByDescending(i => i.InvoicedAt);
                        break;
                    case "description":
                         queryable = isAsc ? queryable.OrderBy(i => i.Description) : queryable.OrderByDescending(i => i.Description);
                        break;
                    case "totalAmount":
                         queryable = isAsc ? queryable.OrderBy(i => i.TotalAmount) : queryable.OrderByDescending(i => i.TotalAmount);
                        break;
                    // Computed sorting (PaymentTotal, Balance) is hard in SQL query without computing it first or having property.
                    // Payments is collection. Sum() in OrderBy might work but perform badly.
                    // Let's try simple computed sort if EF allows, otherwise default.
                    case "paymentTotal":
                         queryable = isAsc ? queryable.OrderBy(i => i.Payments.Sum(p => p.Amount)) : queryable.OrderByDescending(i => i.Payments.Sum(p => p.Amount));
                        break;
                     case "balance":
                         queryable = isAsc ? queryable.OrderBy(i => i.TotalAmount - i.Payments.Sum(p => p.Amount)) : queryable.OrderByDescending(i => i.TotalAmount - i.Payments.Sum(p => p.Amount));
                        break;
                    default:
                        queryable = queryable.OrderByDescending(i => i.InvoicedAt);
                        break;
                }
            }
            else
            {
                queryable = queryable.OrderByDescending(i => i.InvoicedAt);
            }

            var invoices = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            Console.WriteLine($"Found {invoices.Count} invoices for date range {request.StartDate} - {request.EndDate}");

            // ReferenceNumber üzerinden VendorInvoiceId null olan ödemeleri almak için önce
            // tüm invoice numaralarını çekelim ve ayrı bir sorgu ile eşleşenleri bulalım
            var invoiceNumbers = invoices.Select(i => i.InvoiceNumber).ToList();
            var vendorIds = invoices.Select(i => i.VendorId).Distinct().ToList();
            
            // VendorInvoiceId null olan ama ReferenceNumber eşleşen ödemeleri çek
            var orphanPayments = await paymentRepo.AsQueryable()
                .Include(p => p.Currency)
                .Where(p => p.VendorInvoiceId == null 
                    && p.ReferenceNumber != null 
                    && invoiceNumbers.Contains(p.ReferenceNumber)
                    && vendorIds.Contains(p.VendorId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            Console.WriteLine($"Found {orphanPayments.Count} orphan payments matching invoice numbers");

            var reportItems = invoices.Select(i => 
            {
                // Fatura ile direkt ilişkili ödemeler (VendorInvoiceId kullanarak)
                var linkedPayments = i.Payments.ToList();
                
                // ReferenceNumber eşleşmesi ile gelen orphan ödemeler
                var matchingOrphanPayments = orphanPayments
                    .Where(p => p.ReferenceNumber == i.InvoiceNumber && p.VendorId == i.VendorId)
                    .ToList();
                
                // Tüm ödemeleri birleştir (duplicate ID kontrolü ile)
                var allPayments = linkedPayments
                    .Concat(matchingOrphanPayments.Where(op => !linkedPayments.Any(lp => lp.Id == op.Id)))
                    .ToList();
                
                var paymentTotal = allPayments.Sum(p => p.Amount);
                
                return new VendorBalanceReportItemDto
                {
                    CurrAccCode = i.Vendor?.Code ?? string.Empty,
                    CurrAccDescription = i.Vendor?.Name ?? string.Empty,
                    InvoiceNo = i.InvoiceNumber,
                    InvoiceDate = i.InvoicedAt,
                    Currency = i.Currency?.Code ?? string.Empty,
                    Description = i.Description ?? string.Empty,
                    TotalAmount = i.TotalAmount,
                    PaymentTotal = paymentTotal,
                    Balance = i.TotalAmount - paymentTotal,
                    InvoiceFilePath = i.FilePath,
                    PaymentFilePath = allPayments.FirstOrDefault(p => !string.IsNullOrEmpty(p.FilePath))?.FilePath,
                    PaymentHistory = allPayments.Select(p => new PaymentHistoryItemDto
                    {
                        Id = (int)p.Id,
                        Amount = p.Amount,
                        Currency = p.Currency?.Code ?? string.Empty,
                        PaidAt = p.PaidAt,
                        Description = p.Description ?? string.Empty,
                        FilePath = p.FilePath,
                        ReferenceNumber = p.ReferenceNumber
                    }).OrderByDescending(p => p.PaidAt).ToList()
                };
            }).ToList();

            return new VendorBalanceReportResponse
            {
                Data = reportItems,
                TotalCount = totalCount
            };
        }
    }
}
