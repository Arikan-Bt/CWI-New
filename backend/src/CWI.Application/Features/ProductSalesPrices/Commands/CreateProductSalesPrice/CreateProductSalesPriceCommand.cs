using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;

namespace CWI.Application.Features.ProductSalesPrices.Commands.CreateProductSalesPrice;

public class CreateProductSalesPriceCommand : IRequest<int>, IInvalidatesCache
{
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupProductSalesPrices];

    public class CreateProductSalesPriceCommandHandler : IRequestHandler<CreateProductSalesPriceCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductSalesPriceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateProductSalesPriceCommand request, CancellationToken cancellationToken)
        {
            var entity = new ProductSalesPrice
            {
                ProductId = request.ProductId,
                CustomerId = request.CustomerId,
                Price = request.Price,
                CurrencyId = request.CurrencyId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                IsActive = request.IsActive
            };

            await _unitOfWork.Repository<ProductSalesPrice, int>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
