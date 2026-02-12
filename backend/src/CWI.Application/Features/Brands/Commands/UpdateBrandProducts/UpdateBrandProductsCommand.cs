using CWI.Application.Common.Models;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Common;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Brands.Commands.UpdateBrandProducts;

public record UpdateBrandProductsCommand(int BrandId, List<int> ProductIds) : IRequest<Result<int>>;

public class UpdateBrandProductsCommandHandler : IRequestHandler<UpdateBrandProductsCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandProductsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(UpdateBrandProductsCommand request, CancellationToken cancellationToken)
    {
        var brandRepo = _unitOfWork.Repository<Brand, int>();
        var brand = await brandRepo.GetByIdAsync(request.BrandId);
        if (brand == null)
        {
            return Result<int>.Failure($"Brand not found with ID {request.BrandId}");
        }

        // 1. Markaya şu an atanmış ürünleri bul
        var productRepo = _unitOfWork.Repository<Product, int>();
        
        var currentProducts = await productRepo.AsQueryableTracking()
            .Where(p => p.BrandId == request.BrandId)
            .ToListAsync(cancellationToken);

        // 2. Yeni listedeki ürünleri bul
        var newProducts = await productRepo.AsQueryableTracking()
            .Where(p => request.ProductIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        // 3. Unassign logic: Şu an atanmış ama yeni listede olmayanları çıkar
        foreach (var product in currentProducts)
        {
            if (!request.ProductIds.Contains(product.Id))
            {
                product.BrandId = null;
            }
        }

        // 4. Assign logic: Yeni listede olanları markaya ata
        foreach (var product in newProducts)
        {
            product.BrandId = request.BrandId;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Succeed(request.BrandId);
    }
}
