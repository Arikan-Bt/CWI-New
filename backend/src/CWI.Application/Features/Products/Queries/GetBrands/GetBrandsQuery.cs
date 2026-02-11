using CWI.Application.DTOs.Products;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Products.Queries.GetBrands;

public class GetBrandsQuery : IRequest<List<BrandDto>>
{
    public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBrandsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
        {
            var brandRepo = _unitOfWork.Repository<Brand, int>();
            
            return await brandRepo.AsQueryable()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new BrandDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}
