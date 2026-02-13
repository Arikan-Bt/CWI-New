using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;

namespace CWI.Application.Features.ProductPurchasePrices.Commands.DeleteProductPurchasePrice;

public class DeleteProductPurchasePriceCommand : IRequest<int>, IInvalidatesCache
{
    public int Id { get; set; }

    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupProductPurchasePrices];

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

            repository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
