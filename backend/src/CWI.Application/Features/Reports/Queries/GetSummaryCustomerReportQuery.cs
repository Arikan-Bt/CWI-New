using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

/// <summary>
/// Müşteri özet raporunu getiren sorgu
/// </summary>
public class GetSummaryCustomerReportQuery : IRequest<SummaryCustomerReportResponse>
{
    public SummaryCustomerReportRequest Request { get; set; } = null!;

    public class GetSummaryCustomerReportQueryHandler : IRequestHandler<GetSummaryCustomerReportQuery, SummaryCustomerReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSummaryCustomerReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SummaryCustomerReportResponse> Handle(GetSummaryCustomerReportQuery query, CancellationToken cancellationToken)
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

            var groupedData = transactions
                .GroupBy(t => new
                {
                    Code = t.Customer?.Code ?? "-",
                    Name = t.Customer?.Name ?? "Unknown"
                })
                .Select(g => new SummaryCustomerItemDto
                {
                    AccountDescription = g.Key.Code + " - " + g.Key.Name,
                    Debit = g.Sum(x => x.DebitAmount),
                    Credit = g.Sum(x => x.CreditAmount),
                    RecBalance = g.Sum(x => x.DebitAmount) - g.Sum(x => x.CreditAmount),
                    Currency = "$"
                })
                .ToList();

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

            var missingOrdersByCustomer = missingOrders
                .Where(o => !existingOrderReferenceKeys.Contains($"{o.CustomerId}|{o.OrderNumber}"))
                .GroupBy(o => new
                {
                    Code = o.Customer?.Code ?? "-",
                    Name = o.Customer?.Name ?? "Unknown"
                })
                .Select(g => new
                {
                    AccountDescription = g.Key.Code + " - " + g.Key.Name,
                    ExtraDebit = g.Sum(x => x.GrandTotal)
                })
                .ToList();

            foreach (var missing in missingOrdersByCustomer)
            {
                var row = groupedData.FirstOrDefault(x => x.AccountDescription == missing.AccountDescription);
                if (row == null)
                {
                    groupedData.Add(new SummaryCustomerItemDto
                    {
                        AccountDescription = missing.AccountDescription,
                        Debit = missing.ExtraDebit,
                        Credit = 0,
                        RecBalance = missing.ExtraDebit,
                        Currency = "$"
                    });
                }
                else
                {
                    row.Debit += missing.ExtraDebit;
                    row.RecBalance = row.Debit - row.Credit;
                }
            }

            groupedData = ApplySorting(groupedData, request.SortField, request.SortOrder);

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var paged = groupedData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalDebit = groupedData.Sum(x => x.Debit);
            var totalCredit = groupedData.Sum(x => x.Credit);

            return new SummaryCustomerReportResponse
            {
                Data = paged,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                TotalRecBalance = totalDebit - totalCredit,
                TotalCount = groupedData.Count
            };
        }

        private static List<SummaryCustomerItemDto> ApplySorting(
            List<SummaryCustomerItemDto> items,
            string? sortField,
            int sortOrder)
        {
            var asc = sortOrder == 1;

            return sortField switch
            {
                "accountDescription" => asc ? items.OrderBy(x => x.AccountDescription).ToList() : items.OrderByDescending(x => x.AccountDescription).ToList(),
                "debit" => asc ? items.OrderBy(x => x.Debit).ToList() : items.OrderByDescending(x => x.Debit).ToList(),
                "credit" => asc ? items.OrderBy(x => x.Credit).ToList() : items.OrderByDescending(x => x.Credit).ToList(),
                "recBalance" => asc ? items.OrderBy(x => x.RecBalance).ToList() : items.OrderByDescending(x => x.RecBalance).ToList(),
                _ => items.OrderBy(x => x.AccountDescription).ToList()
            };
        }
    }
}
