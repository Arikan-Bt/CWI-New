using CWI.Application.DTOs.Brands;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Brands.Queries.GetBrandById;

/// <summary>
/// Marka detayı sorgusu
/// </summary>
public class GetBrandByIdQuery : IRequest<BrandDetailDto?>
{
    /// <summary>Marka Id</summary>
    public int Id { get; set; }
}

/// <summary>
/// GetBrandByIdQuery handler
/// </summary>
public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BrandDetailDto?> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brandRepo = _unitOfWork.Repository<Brand, int>();

        var brand = await brandRepo.AsQueryable()
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new BrandDetailDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                LogoUrl = x.LogoUrl,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                ProjectType = x.ProjectType
            })
            .FirstOrDefaultAsync(cancellationToken);

        return brand;
    }
}
