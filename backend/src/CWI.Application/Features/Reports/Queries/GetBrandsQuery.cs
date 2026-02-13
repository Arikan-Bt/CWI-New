using CWI.Application.Interfaces.Repositories;
using CWI.Application.Common.Caching;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public class GetBrandsQuery : IRequest<List<BrandLookupDto>>, ICacheableQuery
{
    public string CacheKey => CachePrefixes.LookupBrandsReports;
    public TimeSpan SlidingExpiration => TimeSpan.FromMinutes(2);
    public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    public bool BypassCache { get; init; }
    public bool IsUserScoped => false;
}

public class BrandLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class GetBrandsHandler : IRequestHandler<GetBrandsQuery, List<BrandLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BrandLookupDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Brand>().AsQueryable()
            .Where(b => b.IsActive)
            .Select(b => new BrandLookupDto
            {
                Id = b.Id,
                Name = b.Name
            })
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }
}
