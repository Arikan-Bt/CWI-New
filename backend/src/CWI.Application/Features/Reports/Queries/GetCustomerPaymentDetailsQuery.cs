using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

/// <summary>
/// Müşteri ödeme detay raporunu getiren sorgu
/// </summary>
public class GetCustomerPaymentDetailsQuery : IRequest<CustomerPaymentDetailReportResponse>
{
    public CustomerPaymentDetailReportRequest Request { get; set; } = null!;

    public class GetCustomerPaymentDetailsQueryHandler : IRequestHandler<GetCustomerPaymentDetailsQuery, CustomerPaymentDetailReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerPaymentDetailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerPaymentDetailReportResponse> Handle(GetCustomerPaymentDetailsQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();

            var txQuery = transactionRepo.AsQueryable()
                .Include(t => t.Customer)
                .AsNoTracking();

            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                txQuery = txQuery.Where(t => t.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            if (!string.IsNullOrEmpty(request.CustomerCode))
            {
                txQuery = txQuery.Where(t => t.Customer != null && t.Customer.Code == request.CustomerCode);
            }

            if (request.StartDate.HasValue)
            {
                txQuery = txQuery.Where(t => t.TransactionDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                txQuery = txQuery.Where(t => t.TransactionDate <= request.EndDate.Value);
            }

            var transactions = await txQuery.ToListAsync(cancellationToken);

            var paymentIds = transactions
                .Where(t => t.TransactionType == Domain.Enums.TransactionType.Payment || string.Equals(t.DocumentType, "Payment", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.ApplicationReference)
                .Where(r => long.TryParse(r, out _))
                .Select(r => long.Parse(r!))
                .Distinct()
                .ToList();

            var receiptPathByPaymentId = paymentIds.Count == 0
                ? new Dictionary<long, string?>()
                : await _unitOfWork.Repository<Payment, long>()
                    .AsQueryable()
                    .Where(p => paymentIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.ReceiptFilePath })
                    .ToDictionaryAsync(x => x.Id, x => x.ReceiptFilePath, cancellationToken);

            var detailItems = transactions.Select(t => new CustomerPaymentDetailItemDto
            {
                Date = t.TransactionDate,
                RefNo1 = t.ReferenceNumber ?? string.Empty,
                Description = t.Description ?? string.Empty,
                InvoiceNo = t.ApplicationReference ?? string.Empty,
                DocType = t.DocumentType ?? t.TransactionType.ToString(),
                RefNo2 = t.ApplicationReference ?? string.Empty,
                Debit = t.DebitAmount,
                Credit = t.CreditAmount,
                Balance = t.DebitAmount - t.CreditAmount,
                ReceiptFilePath = long.TryParse(t.ApplicationReference, out var paymentId)
                    && receiptPathByPaymentId.TryGetValue(paymentId, out var path)
                    ? path
                    : null
            }).ToList();

            var existingOrderReferenceKeys = transactions
                .Where(t => t.DebitAmount > 0 && !string.IsNullOrWhiteSpace(t.ReferenceNumber))
                .Select(t => $"{t.CustomerId}|{t.ReferenceNumber}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var orderQuery = _unitOfWork.Repository<Order, long>().AsQueryable()
                .Include(o => o.Customer)
                .AsNoTracking();

            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            if (!string.IsNullOrEmpty(request.CustomerCode))
            {
                orderQuery = orderQuery.Where(o => o.Customer != null && o.Customer.Code == request.CustomerCode);
            }

            if (request.StartDate.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.OrderedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.OrderedAt <= request.EndDate.Value);
            }

            var missingOrders = await orderQuery
                .Where(o => !string.IsNullOrWhiteSpace(o.OrderNumber))
                .Where(o => o.GrandTotal > 0)
                .ToListAsync(cancellationToken);

            foreach (var order in missingOrders)
            {
                var key = $"{order.CustomerId}|{order.OrderNumber}";
                if (existingOrderReferenceKeys.Contains(key))
                {
                    continue;
                }

                detailItems.Add(new CustomerPaymentDetailItemDto
                {
                    Date = order.OrderedAt,
                    RefNo1 = order.OrderNumber,
                    Description = $"Sales Order: {order.OrderNumber}",
                    InvoiceNo = order.Id.ToString(),
                    DocType = "SalesOrder",
                    RefNo2 = order.Id.ToString(),
                    Debit = order.GrandTotal,
                    Credit = 0,
                    Balance = order.GrandTotal
                });
            }

            detailItems = ApplySorting(detailItems, request.SortField, request.SortOrder);

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var pagedData = detailItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalDebit = detailItems.Sum(x => x.Debit);
            var totalCredit = detailItems.Sum(x => x.Credit);

            return new CustomerPaymentDetailReportResponse
            {
                Data = pagedData,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                TotalBalance = totalDebit - totalCredit,
                TotalCount = detailItems.Count
            };
        }

        private static List<CustomerPaymentDetailItemDto> ApplySorting(
            List<CustomerPaymentDetailItemDto> items,
            string? sortField,
            int sortOrder)
        {
            var asc = sortOrder == 1;

            return sortField switch
            {
                "date" => asc ? items.OrderBy(t => t.Date).ToList() : items.OrderByDescending(t => t.Date).ToList(),
                "refNo1" => asc ? items.OrderBy(t => t.RefNo1).ToList() : items.OrderByDescending(t => t.RefNo1).ToList(),
                "description" => asc ? items.OrderBy(t => t.Description).ToList() : items.OrderByDescending(t => t.Description).ToList(),
                "invoiceNo" => asc ? items.OrderBy(t => t.InvoiceNo).ToList() : items.OrderByDescending(t => t.InvoiceNo).ToList(),
                "docType" => asc ? items.OrderBy(t => t.DocType).ToList() : items.OrderByDescending(t => t.DocType).ToList(),
                "debit" => asc ? items.OrderBy(t => t.Debit).ToList() : items.OrderByDescending(t => t.Debit).ToList(),
                "credit" => asc ? items.OrderBy(t => t.Credit).ToList() : items.OrderByDescending(t => t.Credit).ToList(),
                "balance" => asc ? items.OrderBy(t => t.Balance).ToList() : items.OrderByDescending(t => t.Balance).ToList(),
                _ => items.OrderBy(t => t.Date).ToList()
            };
        }
    }
}
