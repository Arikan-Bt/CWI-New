using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;

namespace CWI.Application.Features.ProductSalesPrices.Commands.UpdateProductSalesPrice;

public class UpdateProductSalesPriceCommand : IRequest<int>
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }

    public class UpdateProductSalesPriceCommandHandler : IRequestHandler<UpdateProductSalesPriceCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductSalesPriceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(UpdateProductSalesPriceCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<ProductSalesPrice, int>();
            var entity = await repository.GetByIdAsync(request.Id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"ProductSalesPrice with ID {request.Id} not found.");
            }

            entity.ProductId = request.ProductId;
            entity.CustomerId = request.CustomerId;
            entity.Price = request.Price;
            entity.CurrencyId = request.CurrencyId;
            entity.ValidFrom = request.ValidFrom;
            entity.ValidTo = request.ValidTo;
            entity.IsActive = request.IsActive;

            repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
