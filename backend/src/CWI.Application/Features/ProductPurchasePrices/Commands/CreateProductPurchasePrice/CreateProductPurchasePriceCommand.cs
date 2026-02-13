using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;

namespace CWI.Application.Features.ProductPurchasePrices.Commands.CreateProductPurchasePrice;

public class CreateProductPurchasePriceCommand : IRequest<int>, IInvalidatesCache
{
    public int ProductId { get; set; }
    public int VendorId { get; set; }
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupProductPurchasePrices];

    public class CreateProductPurchasePriceCommandHandler : IRequestHandler<CreateProductPurchasePriceCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductPurchasePriceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateProductPurchasePriceCommand request, CancellationToken cancellationToken)
        {
            var entity = new ProductPurchasePrice
            {
                ProductId = request.ProductId,
                VendorId = request.VendorId,
                Price = request.Price,
                CurrencyId = request.CurrencyId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                IsActive = request.IsActive
            };

            await _unitOfWork.Repository<ProductPurchasePrice, int>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
