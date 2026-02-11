using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CWI.Application.Interfaces.Services;

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
            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
            
            var queryable = transactionRepo.AsQueryable()
                .Include(t => t.Customer)
                .AsNoTracking()
                .Where(t => t.Customer != null && t.Customer.Code == query.CustomerCode);

            // Yönetici değilse ve bağlı bir müşterisi varsa sadece o müşterinin verilerini görür
            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                queryable = queryable.Where(t => t.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            // Group by reference and calculate live balance (debit - credit)
            var references = await queryable
                .Where(t => !string.IsNullOrWhiteSpace(t.ReferenceNumber))
                .GroupBy(t => t.ReferenceNumber)
                .Select(g => new CustomerReferenceDto
                {
                    ReferenceId = g.Key ?? string.Empty,
                    Date = g.Min(x => x.TransactionDate),
                    TotalAmount = g.Sum(x => x.DebitAmount),
                    TotalPayment = g.Sum(x => x.CreditAmount),
                    Balance = g.Sum(x => x.DebitAmount) - g.Sum(x => x.CreditAmount)
                })
                .Where(r => r.TotalAmount > 0)
                .Where(r => r.Balance > 0) // Sadece balance > 0 olanlar
                .OrderByDescending(r => r.Date)
                .ToListAsync(cancellationToken);

            // Fallback for old orders that may not yet have CustomerTransaction records
            var orderRepo = _unitOfWork.Repository<Order, long>();
            var orderQueryable = orderRepo.AsQueryable()
                .AsNoTracking()
                .Include(o => o.Customer)
                .Where(o => o.Customer != null && o.Customer.Code == query.CustomerCode);

            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                orderQueryable = orderQueryable.Where(o => o.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            var missingOrders = await orderQueryable
                .Where(o => !string.IsNullOrWhiteSpace(o.OrderNumber))
                .Where(o => !references.Select(r => r.ReferenceId).Contains(o.OrderNumber))
                .Select(o => new CustomerReferenceDto
                {
                    ReferenceId = o.OrderNumber,
                    Date = o.OrderedAt,
                    TotalAmount = o.GrandTotal,
                    TotalPayment = 0,
                    Balance = o.GrandTotal
                })
                .Where(x => x.TotalAmount > 0 && x.Balance > 0)
                .ToListAsync(cancellationToken);

            references = references
                .Concat(missingOrders)
                .GroupBy(x => x.ReferenceId)
                .Select(g => g.OrderByDescending(x => x.Date).First())
                .OrderByDescending(x => x.Date)
                .ToList();

            return new CustomerReferencesResponse
            {
                Data = references
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
