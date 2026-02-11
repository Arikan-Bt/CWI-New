using CWI.Application.DTOs.Payments;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Payments.Queries;

public class GetPaymentDetailQuery : IRequest<PaymentDetailDto?>
{
    public long Id { get; set; }

    public class GetPaymentDetailQueryHandler : IRequestHandler<GetPaymentDetailQuery, PaymentDetailDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetPaymentDetailQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PaymentDetailDto?> Handle(GetPaymentDetailQuery request, CancellationToken cancellationToken)
        {
            var paymentRepo = _unitOfWork.Repository<Payment, long>();
            
            var query = paymentRepo.AsQueryable()
                .Include(p => p.Customer)
                .Include(p => p.Currency)
                .Include(p => p.PaymentMethod)
                .AsNoTracking();

            var payment = await query.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            
            if (payment == null) return null;

            // Yetki kontrolü
            if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
            {
                if (payment.CustomerId != _currentUserService.LinkedCustomerId.Value)
                    return null;
            }

            return new PaymentDetailDto
            {
                Id = payment.Id,
                CustomerCode = payment.Customer?.Code ?? "-",
                CustomerName = payment.Customer?.Name ?? "Unknown",
                Amount = payment.Amount,
                CurrencyCode = payment.Currency.Code,
                PaidAt = payment.PaidAt,
                Status = payment.Status.ToString(),
                ReceiptNumber = payment.ReceiptNumber,
                PaymentMethod = payment.PaymentMethod.Name,
                ReceiptFilePath = payment.ReceiptFilePath,
                Notes = payment.Notes,
                CreatedBy = payment.CreatedByUsername ?? "Unknown",
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
