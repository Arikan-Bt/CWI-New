using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Brands.Commands.DeleteBrand;

/// <summary>
/// Marka silme komutu (soft delete)
/// </summary>
public class DeleteBrandCommand : IRequest<bool>, IInvalidatesCache
{
    /// <summary>Silinecek marka Id</summary>
    public int Id { get; set; }

    public IReadOnlyCollection<string> CachePrefixesToInvalidate =>
    [
        CachePrefixes.LookupBrandsReports,
        CachePrefixes.LookupBrandsProducts
    ];
}

/// <summary>
/// DeleteBrandCommand handler
/// </summary>
public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brandRepo = _unitOfWork.Repository<Brand, int>();

        var brand = await brandRepo.AsQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (brand == null)
        {
            throw new KeyNotFoundException($"Marka bulunamadi: {request.Id}");
        }

        brandRepo.Delete(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
