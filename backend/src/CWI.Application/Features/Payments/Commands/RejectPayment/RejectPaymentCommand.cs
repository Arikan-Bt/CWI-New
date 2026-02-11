using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Payments;
using CWI.Domain.Enums;
using MediatR;

namespace CWI.Application.Features.Payments.Commands.RejectPayment;

public class RejectPaymentCommand : IRequest<bool>
{
    public long PaymentId { get; set; }
    public string Reason { get; set; } = string.Empty;

    public class RejectPaymentHandler : IRequestHandler<RejectPaymentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RejectPaymentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RejectPaymentCommand request, CancellationToken cancellationToken)
        {
            var paymentRepo = _unitOfWork.Repository<Payment, long>();
            var payment = await paymentRepo.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null) return false;
            
            if (payment.Status == PaymentStatus.Rejected) return true;

            payment.Status = PaymentStatus.Rejected;
            payment.Notes = string.IsNullOrEmpty(payment.Notes) ? $"Red Nedeni: {request.Reason}" : $"{payment.Notes} | Red Nedeni: {request.Reason}";

            paymentRepo.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
