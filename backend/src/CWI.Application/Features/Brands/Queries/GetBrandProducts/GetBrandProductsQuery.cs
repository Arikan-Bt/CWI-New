using CWI.Application.Common.Models;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Common;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Brands.Queries.GetBrandProducts;

public record GetBrandProductsQuery(int BrandId) : IRequest<Result<List<BrandProductDto>>>;

public class GetBrandProductsQueryHandler : IRequestHandler<GetBrandProductsQuery, Result<List<BrandProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<BrandProductDto>>> Handle(GetBrandProductsQuery request, CancellationToken cancellationToken)
    {
        // Tüm aktif ürünleri getir
        // Eğer ürünün BrandId'si istekteki BrandId ile eşleşiyorsa IsSelected = true
        // İsteğe göre: Product listesinde Name alanı Brand Name olarak gösterilmeli denmişti,
        // ancak bu backend tarafında mı yoksa frontend tarafında mı yapılacak?
        // Plan'da "In the list of products assigned to a brand... the 'Product Name' column will display the Brand Name" demiştik.
        // Ama mantıken assignment ekranında hangi ürünü atadığımızı görmemiz lazım, o yüzden orijinal ürün ismini de dönmeliyiz.
        // BrandName alanını da dönelim, frontend istediğini göstersin.

        var productRepo = _unitOfWork.Repository<Product, int>();

        var products = await productRepo.AsQueryable()
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new BrandProductDto
            {
                Id = p.Id,
                Name = p.Name, // Orijinal ürün adı
                Sku = p.Sku,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                IsSelected = p.BrandId == request.BrandId
            })
            .ToListAsync(cancellationToken);

        return Result<List<BrandProductDto>>.Succeed(products);
    }
}
