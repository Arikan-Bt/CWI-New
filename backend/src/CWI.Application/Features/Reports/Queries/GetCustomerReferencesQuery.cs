using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Lookups;
using CWI.Domain.Enums;

namespace CWI.Application.Features.Reports.Queries;

/// <summary>
/// Müşteriye ait balance > 0 olan referansları getiren sorgu
/// Add Payment modal'ında Reference Code seçimi için kullanılır
/// </summary>
public class GetCustomerReferencesQuery : IRequest<CustomerReferencesResponse>
{
    public string CustomerCode { get; set; } = string.Empty;

    public class GetCustomerReferencesQueryHandler : IRequestHandler<GetCustomerReferencesQuery, CustomerReferencesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerReferencesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerReferencesResponse> Handle(GetCustomerReferencesQuery query, CancellationToken cancellationToken)
        {
            var customerRepo = _unitOfWork.Repository<Customer, int>();
            var customer = await customerRepo.AsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == query.CustomerCode, cancellationToken);

            if (customer == null) return new CustomerReferencesResponse();

            // 1. Get all orders for this customer to determine their status and assigned numbers
            var orderRepo = _unitOfWork.Repository<Order, long>();
            var ordersQueryable = orderRepo.AsQueryable()
                .AsNoTracking()
                .Where(o => o.CustomerId == customer.Id);

            // Access control
            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                ordersQueryable = ordersQueryable.Where(o => o.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            var orders = await ordersQueryable.ToListAsync(cancellationToken);
            var cancelledOrderIds = orders.Where(o => o.Status == OrderStatus.Canceled).Select(o => o.Id).ToHashSet();
            
            // 2. Get Assigned Invoice Numbers
            var assignedNumbersList = await _unitOfWork.Repository<LocalizedString>().AsQueryable()
                .AsNoTracking()
                .Where(x => x.Module == "AssignedOrders")
                .ToListAsync(cancellationToken);

            var orderIdToAssignedNo = new Dictionary<long, string>();
            var assignedNoToOrderId = new Dictionary<string, long>();

            foreach (var item in assignedNumbersList)
            {
                if (item.Key.StartsWith("Assigned_No_") && long.TryParse(item.Key.Replace("Assigned_No_", ""), out long orderId))
                {
                    orderIdToAssignedNo[orderId] = item.Value;
                    assignedNoToOrderId[item.Value] = orderId;
                }
            }

            var orderNumberToOrderId = orders.ToDictionary(o => o.OrderNumber, o => o.Id);

            // 3. Get Transactions
            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
            var transactions = await transactionRepo.AsQueryable()
                .Include(t => t.Customer)
                .AsNoTracking()
                .Where(t => t.CustomerId == customer.Id)
                .ToListAsync(cancellationToken);

            // 4. Group Transactions by a common key (OrderId if applicable, otherwise ReferenceNumber)
            var consolidatedGroups = transactions
                .Select(t => {
                    long? relatedOrderId = null;
                    
                    // Try to find OrderId by ApplicationReference (Order creation transactions)
                    if (long.TryParse(t.ApplicationReference, out long appId) && orders.Any(o => o.Id == appId))
                    {
                        relatedOrderId = appId;
                    }
                    // Try to find OrderId by ReferenceNumber (Invoice Number in payments)
                    else if (!string.IsNullOrEmpty(t.ReferenceNumber) && assignedNoToOrderId.TryGetValue(t.ReferenceNumber, out long oidByAssigned))
                    {
                        relatedOrderId = oidByAssigned;
                    }
                    // Try to find OrderId by ReferenceNumber (Order Number in old payments)
                    else if (!string.IsNullOrEmpty(t.ReferenceNumber) && orderNumberToOrderId.TryGetValue(t.ReferenceNumber, out long oidByRef))
                    {
                        relatedOrderId = oidByRef;
                    }

                    // Determine Display Reference
                    string displayRef = t.ReferenceNumber ?? string.Empty;
                    if (relatedOrderId.HasValue)
                    {
                        if (orderIdToAssignedNo.TryGetValue(relatedOrderId.Value, out var assignedNo))
                        {
                            displayRef = assignedNo;
                        }
                        else
                        {
                            var order = orders.FirstOrDefault(o => o.Id == relatedOrderId.Value);
                            if (order != null) displayRef = order.OrderNumber;
                        }
                    }

                    return new { Transaction = t, RelatedOrderId = relatedOrderId, DisplayRef = displayRef };
                })
                // Rule: "Cancel olan işlemler gelmiyecek"
                .Where(x => !x.RelatedOrderId.HasValue || !cancelledOrderIds.Contains(x.RelatedOrderId.Value))
                .GroupBy(x => x.DisplayRef)
                .Select(g => new CustomerReferenceDto
                {
                    ReferenceId = g.Key,
                    Date = g.Min(x => x.Transaction.TransactionDate),
                    TotalAmount = g.Sum(x => x.Transaction.DebitAmount),
                    TotalPayment = g.Sum(x => x.Transaction.CreditAmount),
                    Balance = g.Sum(x => x.Transaction.DebitAmount) - g.Sum(x => x.Transaction.CreditAmount)
                })
                .Where(r => r.TotalAmount > 0 && r.Balance > 0)
                .ToList();

            // 5. Fallback for orders that have no transactions yet
            var ordersWithTransactions = consolidatedGroups.Select(g => g.ReferenceId).ToHashSet();
            
            var missingOrders = orders
                .Where(o => o.Status != OrderStatus.Canceled && !string.IsNullOrWhiteSpace(o.OrderNumber))
                .Select(o => {
                    var displayRef = orderIdToAssignedNo.ContainsKey(o.Id) ? orderIdToAssignedNo[o.Id] : o.OrderNumber;
                    return new { Order = o, DisplayRef = displayRef };
                })
                .Where(x => !ordersWithTransactions.Contains(x.DisplayRef))
                .Select(x => new CustomerReferenceDto
                {
                    ReferenceId = x.DisplayRef,
                    Date = x.Order.OrderedAt,
                    TotalAmount = x.Order.GrandTotal,
                    TotalPayment = 0,
                    Balance = x.Order.GrandTotal
                })
                .Where(r => r.TotalAmount > 0 && r.Balance > 0)
                .ToList();

            var finalResult = consolidatedGroups
                .Concat(missingOrders)
                .GroupBy(x => x.ReferenceId)
                .Select(g => g.OrderByDescending(x => x.Date).First())
                .OrderByDescending(x => x.Date)
                .ToList();

            return new CustomerReferencesResponse
            {
                Data = finalResult
            };
        }

    }
}

/// <summary>
/// Müşteri referans DTO'su
/// </summary>
public class CustomerReferenceDto
{
    /// <summary>
    /// Referans numarası (Sipariş numarası)
    /// </summary>
    public string ReferenceId { get; set; } = string.Empty;
    
    /// <summary>
    /// İşlem tarihi
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Toplam ödeme
    /// </summary>
    public decimal TotalPayment { get; set; }
    
    /// <summary>
    /// Kalan bakiye
    /// </summary>
    public decimal Balance { get; set; }
}

/// <summary>
/// Müşteri referansları response DTO'su
/// </summary>
public class CustomerReferencesResponse
{
    public List<CustomerReferenceDto> Data { get; set; } = new();
}
