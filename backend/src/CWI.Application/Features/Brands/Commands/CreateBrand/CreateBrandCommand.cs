using CWI.Application.Common.Caching;
using CWI.Application.DTOs.Brands;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using FluentValidation;
using MediatR;

namespace CWI.Application.Features.Brands.Commands.CreateBrand;

/// <summary>
/// Yeni marka olusturma komutu
/// </summary>
public class CreateBrandCommand : IRequest<BrandDetailDto>, IInvalidatesCache
{
    /// <summary>Marka olusturma verileri</summary>
    public CreateBrandDto Data { get; set; } = null!;

    public IReadOnlyCollection<string> CachePrefixesToInvalidate =>
    [
        CachePrefixes.LookupBrandsReports,
        CachePrefixes.LookupBrandsProducts
    ];
}

/// <summary>
/// CreateBrandCommand icin dogrulayici
/// </summary>
public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Data.Code)
            .NotEmpty().WithMessage("Marka kodu bos olamaz")
            .MaximumLength(50).WithMessage("Marka kodu 50 karakteri gecemez");

        RuleFor(x => x.Data.Name)
            .NotEmpty().WithMessage("Marka adi bos olamaz")
            .MaximumLength(200).WithMessage("Marka adi 200 karakteri gecemez");
    }
}

/// <summary>
/// CreateBrandCommand handler
/// </summary>
public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, BrandDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BrandDetailDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brandRepo = _unitOfWork.Repository<Brand, int>();

        var brand = new Brand
        {
            Code = request.Data.Code,
            Name = request.Data.Name,
            LogoUrl = request.Data.LogoUrl,
            SortOrder = request.Data.SortOrder,
            IsActive = request.Data.IsActive,
            ProjectType = request.Data.ProjectType
        };

        await brandRepo.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BrandDetailDto
        {
            Id = brand.Id,
            Code = brand.Code,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl,
            SortOrder = brand.SortOrder,
            IsActive = brand.IsActive,
            ProjectType = brand.ProjectType
        };
    }
}
