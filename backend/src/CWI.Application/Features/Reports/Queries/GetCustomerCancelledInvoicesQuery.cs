using CWI.Application.DTOs.Reports;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Lookups;
using CWI.Domain.Entities.Orders;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

/// <summary>
/// Seçilen müşteriye ait cancel edilmiş ve ödeme almış invoice seçeneklerini getirir.
/// </summary>
public class GetCustomerCancelledInvoicesQuery : IRequest<CancelledInvoicesResponse>
{
    public string CustomerCode { get; set; } = string.Empty;

    public class GetCustomerCancelledInvoicesQueryHandler : IRequestHandler<GetCustomerCancelledInvoicesQuery, CancelledInvoicesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerCancelledInvoicesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CancelledInvoicesResponse> Handle(GetCustomerCancelledInvoicesQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerCode))
            {
                return new CancelledInvoicesResponse();
            }

            var customer = await _unitOfWork.Repository<Customer, int>()
                .AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == request.CustomerCode, cancellationToken);

            if (customer == null)
            {
                return new CancelledInvoicesResponse();
            }

            var orderQuery = _unitOfWork.Repository<Order, long>()
                .AsQueryable()
                .AsNoTracking()
                .Where(o => o.CustomerId == customer.Id && o.Status == OrderStatus.Canceled);

            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            var cancelledOrders = await orderQuery.ToListAsync(cancellationToken);
            if (cancelledOrders.Count == 0)
            {
                return new CancelledInvoicesResponse();
            }

            var orderIds = cancelledOrders.Select(o => o.Id).ToHashSet();
            var orderNumberToOrderId = cancelledOrders
                .Where(o => !string.IsNullOrWhiteSpace(o.OrderNumber))
                .ToDictionary(o => o.OrderNumber, o => o.Id, StringComparer.OrdinalIgnoreCase);

            var assignedNumberRows = await _unitOfWork.Repository<LocalizedString>()
                .AsQueryable()
                .AsNoTracking()
                .Where(x => x.Module == "AssignedOrders")
                .ToListAsync(cancellationToken);

            var orderIdToInvoiceNo = new Dictionary<long, string>();
            var invoiceNoToOrderId = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in assignedNumberRows)
            {
                if (!row.Key.StartsWith("Assigned_No_"))
                {
                    continue;
                }

                if (!long.TryParse(row.Key.Replace("Assigned_No_", string.Empty), out var orderId))
                {
                    continue;
                }

                if (!orderIds.Contains(orderId))
                {
                    continue;
                }

                orderIdToInvoiceNo[orderId] = row.Value;
                invoiceNoToOrderId[row.Value] = orderId;
            }

            var transactions = await _unitOfWork.Repository<CustomerTransaction, long>()
                .AsQueryable()
                .AsNoTracking()
                .Where(t => t.CustomerId == customer.Id && t.CreditAmount > 0)
                .ToListAsync(cancellationToken);

            var paidAmountByOrderId = new Dictionary<long, decimal>();

            foreach (var tx in transactions)
            {
                long? relatedOrderId = null;

                if (long.TryParse(tx.ApplicationReference, out var appOrderId) && orderIds.Contains(appOrderId))
                {
                    relatedOrderId = appOrderId;
                }
                else if (!string.IsNullOrWhiteSpace(tx.ReferenceNumber) &&
                         invoiceNoToOrderId.TryGetValue(tx.ReferenceNumber, out var orderIdByInvoice))
                {
                    relatedOrderId = orderIdByInvoice;
                }
                else if (!string.IsNullOrWhiteSpace(tx.ReferenceNumber) &&
                         orderNumberToOrderId.TryGetValue(tx.ReferenceNumber, out var orderIdByOrderNumber))
                {
                    relatedOrderId = orderIdByOrderNumber;
                }

                if (!relatedOrderId.HasValue)
                {
                    continue;
                }

                if (!paidAmountByOrderId.ContainsKey(relatedOrderId.Value))
                {
                    paidAmountByOrderId[relatedOrderId.Value] = 0;
                }

                paidAmountByOrderId[relatedOrderId.Value] += tx.CreditAmount;
            }

            var result = cancelledOrders
                .Select(order =>
                {
                    var paidAmount = paidAmountByOrderId.TryGetValue(order.Id, out var amount) ? amount : 0m;
                    var invoiceNo = orderIdToInvoiceNo.TryGetValue(order.Id, out var assignedInvoiceNo)
                        ? assignedInvoiceNo
                        : order.OrderNumber;

                    return new CancelledInvoiceOptionDto
                    {
                        OrderId = order.Id,
                        InvoiceNo = invoiceNo ?? string.Empty,
                        CanceledDate = order.UpdatedAt ?? order.CreatedAt,
                        PaidAmount = paidAmount
                    };
                })
                .Where(x => x.PaidAmount > 0)
                .OrderByDescending(x => x.CanceledDate)
                .ToList();

            return new CancelledInvoicesResponse { Data = result };
        }
    }
}
