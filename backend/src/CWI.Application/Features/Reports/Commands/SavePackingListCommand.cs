using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CWI.Application.Features.Reports.Commands;

public class SavePackingListCommand : IRequest<bool>
{
    public long OrderId { get; set; }
    public List<SavePackingListItemDto> Items { get; set; } = new();
    public List<SavePackingListCartonDto> Cartons { get; set; } = new();
}

public class SavePackingListItemDto
{
    public long OrderItemId { get; set; }
    public string CartonNo { get; set; } = string.Empty;
    public int Qty { get; set; }
}

public class SavePackingListCartonDto
{
    public long Id { get; set; }
    public string CartonNo { get; set; } = string.Empty;
    public decimal? NetWeight { get; set; }
    public decimal? GrossWeight { get; set; }
    public string Measurements { get; set; } = string.Empty;
}

public class SavePackingListCommandHandler : IRequestHandler<SavePackingListCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public SavePackingListCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SavePackingListCommand request, CancellationToken cancellationToken)
    {
        var orderCallback = await _unitOfWork.Repository<Order, long>().GetByIdAsync(request.OrderId, cancellationToken);
            
        if (orderCallback == null) return false;

        var packageRepo = _unitOfWork.Repository<OrderPackage, long>();
        var packageItemRepo = _unitOfWork.Repository<OrderPackageItem, long>();

        // 1. Process Cartons
        // Fetch existing
        var existingCartons = await packageRepo.AsQueryable()
            .Include(x => x.Items)
            .Where(x => x.OrderId == request.OrderId)
            .ToListAsync(cancellationToken);

        // Identify deletions
        var validCartonIds = request.Cartons.Where(c => c.Id > 0).Select(c => c.Id).ToList();
        var cartonsToDelete = existingCartons.Where(c => c.Id > 0 && !validCartonIds.Contains(c.Id)).ToList();
        
        if (cartonsToDelete.Any())
        {
            packageRepo.DeleteRange(cartonsToDelete);
        }

        // Update/Create
        foreach (var cartonDto in request.Cartons)
        {
            if (cartonDto.Id > 0)
            {
                var package = existingCartons.FirstOrDefault(c => c.Id == cartonDto.Id);
                if (package == null) continue; // Should not happen
                
                // Update properties manually or via mapper, here manual
                UpdatePackageProperties(package, cartonDto);
                packageRepo.Update(package);
            }
            else
            {
                var package = new OrderPackage { OrderId = request.OrderId };
                UpdatePackageProperties(package, cartonDto);
                await packageRepo.AddAsync(package, cancellationToken);
            }
        }
        
        // Save to persist packages and get IDs
        await _unitOfWork.SaveChangesAsync(cancellationToken); 

        // 2. Process Items matches
        // Refresh packages list to get new IDs
        var currentCartons = await packageRepo.AsQueryable()
             .Include(p => p.Items)
             .Where(p => p.OrderId == request.OrderId)
             .ToListAsync(cancellationToken);
             
        // Clear existing links
        var allPackageItems = currentCartons.SelectMany(c => c.Items).ToList();
        if (allPackageItems.Any())
        {
            packageItemRepo.DeleteRange(allPackageItems);
            // We need to save changes here to remove constraints before re-adding?
            // Or EF Core detects changes. Let's do it in one go if possible, but existing items are tracked.
            // Safe bet with EF Core is usually fine unless unique constraints.
        }
        
        // Re-create links
        foreach (var itemDto in request.Items)
        {
             if (string.IsNullOrWhiteSpace(itemDto.CartonNo)) continue;
             
             // Support multiple cartoons split by comma
             var cartonNos = itemDto.CartonNo.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
             
             foreach(var cNo in cartonNos) 
             {
                 var targetCarton = currentCartons.FirstOrDefault(c => c.PackageNumber == cNo);
                 if (targetCarton == null) continue;
                 
                 var relation = new OrderPackageItem
                 {
                      OrderPackageId = targetCarton.Id,
                      OrderItemId = itemDto.OrderItemId,
                      Quantity = itemDto.Qty 
                 };
                 await packageItemRepo.AddAsync(relation, cancellationToken);
             }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void UpdatePackageProperties(OrderPackage pkg, SavePackingListCartonDto dto)
    {
        pkg.PackageNumber = dto.CartonNo;
        pkg.NetWeight = dto.NetWeight;
        pkg.Weight = dto.GrossWeight; // Legacy mapping
        pkg.GrossWeight = dto.GrossWeight;
        ParseMeasurements(dto.Measurements, pkg);
    }

    private void ParseMeasurements(string measure, OrderPackage pkg)
    {
         if (string.IsNullOrWhiteSpace(measure)) return;
         var parts = measure.Split('*');
         if (parts.Length >= 1 && decimal.TryParse(parts[0], out var l)) pkg.Length = l;
         if (parts.Length >= 2 && decimal.TryParse(parts[1], out var w)) pkg.Width = w;
         if (parts.Length >= 3 && decimal.TryParse(parts[2], out var h)) pkg.Height = h;
    }
}
