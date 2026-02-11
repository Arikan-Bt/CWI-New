using CWI.Application.DTOs.Payments;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Payments.Queries;

public class PaymentListResponse
{
    public List<PaymentListDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

public class GetPaymentsQuery : IRequest<PaymentListResponse>
{
    public PaymentFilterDto Filter { get; set; } = new();

    public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PaymentListResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetPaymentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PaymentListResponse> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var paymentRepo = _unitOfWork.Repository<Payment, long>();
            
            var query = paymentRepo.AsQueryable()
                .Include(p => p.Customer)
                .Include(p => p.Currency)
                .Include(p => p.PaymentMethod)
                .AsNoTracking();

            // Yetki Kontrolü
            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == _currentUserService.LinkedCustomerId.Value);
            }

            // Filtreler
            if (filter.StartDate.HasValue)
                query = query.Where(p => p.PaidAt >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(p => p.PaidAt <= filter.EndDate.Value);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (!string.IsNullOrEmpty(filter.CurrAccCode))
                query = query.Where(p => p.Customer != null && p.Customer.Code.Contains(filter.CurrAccCode));

            // Sıralama
            query = query.OrderByDescending(p => p.PaidAt);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new PaymentListDto
                {
                    Id = p.Id,
                    CustomerCode = p.Customer != null ? p.Customer.Code : "-",
                    CustomerName = p.Customer != null ? p.Customer.Name : "Unknown",
                    Amount = p.Amount,
                    CurrencyCode = p.Currency.Code,
                    PaidAt = p.PaidAt,
                    Status = p.Status.ToString(),
                    ReceiptNumber = p.ReceiptNumber,
                    PaymentMethod = p.PaymentMethod.Name,
                    ReceiptFilePath = p.ReceiptFilePath,
                    Notes = p.Notes
                })
                .ToListAsync(cancellationToken);

            return new PaymentListResponse
            {
                Data = items,
                TotalCount = totalCount
            };
        }
    }
}
