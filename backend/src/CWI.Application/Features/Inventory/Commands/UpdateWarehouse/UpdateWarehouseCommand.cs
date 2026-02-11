using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Inventory.Commands.UpdateWarehouse;

/// <summary>
/// Depo güncelleme komutu
/// </summary>
public class UpdateWarehouseCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }

    public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateWarehouseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();

            // Depoyu bul
            var warehouse = await warehouseRepo.GetByIdAsync(request.Id, cancellationToken);
            if (warehouse == null)
            {
                throw new InvalidOperationException($"Warehouse with ID {request.Id} not found.");
            }

            // Code unique kontrolü (kendi ID'si hariç)
            var existingWarehouse = await warehouseRepo
                .FirstOrDefaultAsync(w => w.Code == request.Code && w.Id != request.Id, cancellationToken);

            if (existingWarehouse != null)
            {
                throw new InvalidOperationException($"Warehouse with code '{request.Code}' already exists.");
            }

            // Eğer IsDefault true ise, diğer tüm depoların IsDefault'unu false yap
            if (request.IsDefault && !warehouse.IsDefault)
            {
                var allWarehouses = await warehouseRepo
                    .AsQueryable()
                    .Where(w => w.IsDefault && w.Id != request.Id)
                    .ToListAsync(cancellationToken);

                foreach (var wh in allWarehouses)
                {
                    wh.IsDefault = false;
                }
            }

            // Güncelle
            warehouse.Code = request.Code;
            warehouse.Name = request.Name;
            warehouse.Address = request.Address;
            warehouse.IsActive = request.IsActive;
            warehouse.IsDefault = request.IsDefault;

            warehouseRepo.Update(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
