using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

/// <summary>
/// Müşteri cari hareket raporunu getiren sorgu
/// </summary>
public class GetCustomerBalanceReportQuery : IRequest<CustomerBalanceReportResponse>
{
    public CustomerBalanceReportRequest Request { get; set; } = null!;

    public class GetCustomerBalanceReportQueryHandler : IRequestHandler<GetCustomerBalanceReportQuery, CustomerBalanceReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerBalanceReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerBalanceReportResponse> Handle(GetCustomerBalanceReportQuery query, CancellationToken cancellationToken)
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

            if (request.StartDate.HasValue)
            {
                txQuery = txQuery.Where(t => t.TransactionDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                txQuery = txQuery.Where(t => t.TransactionDate <= request.EndDate.Value);
            }

            var transactions = await txQuery.ToListAsync(cancellationToken);

            var reportItems = transactions.Select(t => new CustomerBalanceReportItemDto
            {
                CurrAccCode = t.Customer?.Code ?? "-",
                CurrAccDescription = t.Customer?.Name ?? "Unknown",
                Date = t.TransactionDate,
                ReferenceId = t.ReferenceNumber ?? string.Empty,
                TotalAmount = t.DebitAmount,
                TotalPayment = t.CreditAmount,
                Balance = t.Balance,
                OrderStatus = t.TransactionType.ToString(),
                Status = (t.DebitAmount - t.CreditAmount) != 0 ? "Open" : "Closed"
            }).ToList();

            // Fallback: legacy orders that do not have CustomerTransaction row yet
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

                reportItems.Add(new CustomerBalanceReportItemDto
                {
                    CurrAccCode = order.Customer?.Code ?? "-",
                    CurrAccDescription = order.Customer?.Name ?? "Unknown",
                    Date = order.OrderedAt,
                    ReferenceId = order.OrderNumber,
                    TotalAmount = order.GrandTotal,
                    TotalPayment = 0,
                    Balance = order.GrandTotal,
                    OrderStatus = order.Status.ToString(),
                    Status = order.GrandTotal != 0 ? "Open" : "Closed"
                });
            }

            reportItems = ApplySorting(reportItems, request.SortField, request.SortOrder);

            var totalCount = reportItems.Count;
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var paged = reportItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new CustomerBalanceReportResponse
            {
                Data = paged,
                TotalCount = totalCount
            };
        }

        private static List<CustomerBalanceReportItemDto> ApplySorting(
            List<CustomerBalanceReportItemDto> items,
            string? sortField,
            int sortOrder)
        {
            var asc = sortOrder == 1;

            return sortField switch
            {
                "currAccCode" => asc ? items.OrderBy(t => t.CurrAccCode).ToList() : items.OrderByDescending(t => t.CurrAccCode).ToList(),
                "currAccDescription" => asc ? items.OrderBy(t => t.CurrAccDescription).ToList() : items.OrderByDescending(t => t.CurrAccDescription).ToList(),
                "date" => asc ? items.OrderBy(t => t.Date).ToList() : items.OrderByDescending(t => t.Date).ToList(),
                "referenceId" => asc ? items.OrderBy(t => t.ReferenceId).ToList() : items.OrderByDescending(t => t.ReferenceId).ToList(),
                "totalAmount" => asc ? items.OrderBy(t => t.TotalAmount).ToList() : items.OrderByDescending(t => t.TotalAmount).ToList(),
                "totalPayment" => asc ? items.OrderBy(t => t.TotalPayment).ToList() : items.OrderByDescending(t => t.TotalPayment).ToList(),
                "balance" => asc ? items.OrderBy(t => t.Balance).ToList() : items.OrderByDescending(t => t.Balance).ToList(),
                "orderStatus" => asc ? items.OrderBy(t => t.OrderStatus).ToList() : items.OrderByDescending(t => t.OrderStatus).ToList(),
                _ => items.OrderByDescending(t => t.Date).ToList()
            };
        }
    }
}
