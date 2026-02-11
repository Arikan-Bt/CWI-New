using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Inventory.Commands.DeleteWarehouse;

/// <summary>
/// Depo silme komutu (Soft Delete)
/// </summary>
public class DeleteWarehouseCommand : IRequest<bool>
{
    public int Id { get; set; }

    public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteWarehouseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();

            // Depoyu bul
            var warehouse = await warehouseRepo.GetByIdAsync(request.Id, cancellationToken);
            if (warehouse == null)
            {
                throw new InvalidOperationException($"Warehouse with ID {request.Id} not found.");
            }

            // Default depo silinemez kontrolü
            if (warehouse.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete the default warehouse. Please set another warehouse as default first.");
            }

            // Depoda stok var mı kontrolü (opsiyonel uyarı)
            var inventoryRepo = _unitOfWork.Repository<InventoryItem, long>();
            var hasInventory = await inventoryRepo
                .AsQueryable()
                .AnyAsync(i => i.WarehouseId == request.Id, cancellationToken);

            if (hasInventory)
            {
                throw new InvalidOperationException("Cannot delete warehouse with existing inventory items. Please transfer or remove items first.");
            }

            // Soft delete
            warehouse.IsActive = false;
            warehouseRepo.Update(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
