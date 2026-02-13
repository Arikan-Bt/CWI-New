using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Inventory.Commands.CreateWarehouse;

/// <summary>
/// Yeni depo olusturma komutu
/// </summary>
public class CreateWarehouseCommand : IRequest<int>, IInvalidatesCache
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsDefault { get; set; }

    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupWarehouses];

    public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateWarehouseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouseRepo = _unitOfWork.Repository<Warehouse, int>();

            var existingWarehouse = await warehouseRepo
                .FirstOrDefaultAsync(w => w.Code == request.Code, cancellationToken);

            if (existingWarehouse != null)
            {
                throw new InvalidOperationException($"Warehouse with code '{request.Code}' already exists.");
            }

            if (request.IsDefault)
            {
                var allWarehouses = await warehouseRepo
                    .AsQueryable()
                    .Where(w => w.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var wh in allWarehouses)
                {
                    wh.IsDefault = false;
                }
            }

            var warehouse = new Warehouse
            {
                Code = request.Code,
                Name = request.Name,
                Address = request.Address,
                IsActive = true,
                IsDefault = request.IsDefault
            };

            await warehouseRepo.AddAsync(warehouse, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return warehouse.Id;
        }
    }
}
