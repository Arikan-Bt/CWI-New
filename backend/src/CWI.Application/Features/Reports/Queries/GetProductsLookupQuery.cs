using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public class GetProductsLookupQuery : IRequest<List<ProductLookupDto>>
{
}

public class ProductLookupDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class GetProductsLookupQueryHandler : IRequestHandler<GetProductsLookupQuery, List<ProductLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsLookupQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductLookupDto>> Handle(GetProductsLookupQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Repository<Product>().AsQueryable()
            .Where(p => p.IsActive)
            .Include(p => p.Brand)
            .Select(p => new
            {
                p.Sku,
                p.Name,
                BrandName = p.Brand != null ? p.Brand.Name : null
            })
            .ToListAsync(cancellationToken);

        return products
            .Select(p => new ProductLookupDto
            {
                Label = !string.IsNullOrEmpty(p.BrandName) ? $"[{p.BrandName}] {p.Sku} - {p.Name}" : $"{p.Sku} - {p.Name}",
                Value = p.Sku
            })
            .GroupBy(p => p.Value)
            .Select(g => g.First())
            .OrderBy(p => p.Label)
            .ToList();
    }
}
