using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Payments.Commands.ApprovePayment;

public class ApprovePaymentCommand : IRequest<bool>
{
    public long PaymentId { get; set; }

    public class ApprovePaymentHandler : IRequestHandler<ApprovePaymentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApprovePaymentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ApprovePaymentCommand request, CancellationToken cancellationToken)
        {
            var paymentRepo = _unitOfWork.Repository<Payment, long>();
            var payment = await paymentRepo.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null) return false;
            
            if (payment.Status == PaymentStatus.Completed) return true;

            payment.Status = PaymentStatus.Completed;
            
            // Cari hareket (CustomerTransaction) kaydı oluştur
            var transactionRepo = _unitOfWork.Repository<CustomerTransaction, long>();
            
            // Son bakiyeyi al
            var lastBalance = await transactionRepo.AsQueryable()
                .Where(t => t.CustomerId == payment.CustomerId)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .Select(t => t.Balance)
                .FirstOrDefaultAsync(cancellationToken);

            var newBalance = lastBalance - payment.Amount; // Ödeme (Credit) bakiyeyi düşürür

            var transaction = new CustomerTransaction
            {
                CustomerId = payment.CustomerId,
                TransactionType = TransactionType.Payment,
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = payment.ReceiptNumber ?? payment.Id.ToString(),
                Description = $"Ödeme Tahsilatı - {payment.Id} ({payment.Notes})",
                DocumentType = "Ödeme",
                CreditAmount = payment.Amount,
                DebitAmount = 0,
                Balance = newBalance,
                CreatedAt = DateTime.UtcNow
            };

            transactionRepo.Add(transaction);
            paymentRepo.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
