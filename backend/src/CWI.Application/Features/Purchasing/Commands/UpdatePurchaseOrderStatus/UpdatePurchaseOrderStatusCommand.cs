using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Purchasing;
using MediatR;

namespace CWI.Application.Features.Purchasing.Commands.UpdatePurchaseOrderStatus;

public class UpdatePurchaseOrderStatusCommand : IRequest<bool>
{
    public long Id { get; set; }
    public string Status { get; set; } = string.Empty;

    public class UpdatePurchaseOrderStatusCommandHandler : IRequestHandler<UpdatePurchaseOrderStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePurchaseOrderStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdatePurchaseOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<PurchaseOrder, long>();
            var order = await repo.GetByIdAsync(request.Id);

            if (order == null) return false;

            order.IsReceived = request.Status == "Inactive";
            
            repo.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
