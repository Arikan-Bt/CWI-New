using CWI.Application.DTOs.Inventory;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using MediatR;

namespace CWI.Application.Features.Inventory.Queries.GetWarehouseById;

/// <summary>
/// ID'ye göre depo detayını getir
/// </summary>
public class GetWarehouseByIdQuery : IRequest<WarehouseDetailDto?>
{
    public int Id { get; set; }

    public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDetailDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWarehouseByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WarehouseDetailDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
        {
            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();
            var warehouse = await warehouseRepo.GetByIdAsync(request.Id, cancellationToken);

            if (warehouse == null)
            {
                return null;
            }

            return new WarehouseDetailDto
            {
                Id = warehouse.Id,
                Code = warehouse.Code,
                Name = warehouse.Name,
                Address = warehouse.Address,
                IsActive = warehouse.IsActive,
                IsDefault = warehouse.IsDefault,
                CreatedAt = warehouse.CreatedAt,
                ModifiedAt = warehouse.UpdatedAt
            };
        }
    }
}
