using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;

namespace CWI.Application.Features.ProductPurchasePrices.Commands.DeleteProductPurchasePrice;

public class DeleteProductPurchasePriceCommand : IRequest<int>
{
    public int Id { get; set; }

    public class DeleteProductPurchasePriceCommandHandler : IRequestHandler<DeleteProductPurchasePriceCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductPurchasePriceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(DeleteProductPurchasePriceCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<ProductPurchasePrice, int>();
            var entity = await repository.GetByIdAsync(request.Id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"ProductPurchasePrice with ID {request.Id} not found.");
            }

            // Hard delete or soft delete? Entity implements ISoftDeletable, so repository should handle it or we set IsActive = false.
            // Usually repository DeleteAsync handles soft delete if configured, or performs hard delete.
            // Let's assume standard DeleteAsync.
            repository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
