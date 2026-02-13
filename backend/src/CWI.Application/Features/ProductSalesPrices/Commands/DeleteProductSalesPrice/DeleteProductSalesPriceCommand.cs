using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;

namespace CWI.Application.Features.ProductSalesPrices.Commands.DeleteProductSalesPrice;

public class DeleteProductSalesPriceCommand : IRequest<int>, IInvalidatesCache
{
    public int Id { get; set; }

    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupProductSalesPrices];

    public class DeleteProductSalesPriceCommandHandler : IRequestHandler<DeleteProductSalesPriceCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductSalesPriceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(DeleteProductSalesPriceCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<ProductSalesPrice, int>();
            var entity = await repository.GetByIdAsync(request.Id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"ProductSalesPrice with ID {request.Id} not found.");
            }

            repository.Delete(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
