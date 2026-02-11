using CWI.Application.DTOs.Brands;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Brands.Commands.UpdateBrand;

/// <summary>
/// Marka güncelleme komutu
/// </summary>
public class UpdateBrandCommand : IRequest<BrandDetailDto>
{
    /// <summary>Marka güncelleme verileri</summary>
    public UpdateBrandDto Data { get; set; } = null!;
}

/// <summary>
/// UpdateBrandCommand için doğrulayıcı
/// </summary>
public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Data.Id)
            .GreaterThan(0).WithMessage("Geçerli bir marka Id gerekli");

        RuleFor(x => x.Data.Code)
            .NotEmpty().WithMessage("Marka kodu boş olamaz")
            .MaximumLength(50).WithMessage("Marka kodu 50 karakteri geçemez");

        RuleFor(x => x.Data.Name)
            .NotEmpty().WithMessage("Marka adı boş olamaz")
            .MaximumLength(200).WithMessage("Marka adı 200 karakteri geçemez");
    }
}

/// <summary>
/// UpdateBrandCommand handler
/// </summary>
public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, BrandDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BrandDetailDto> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brandRepo = _unitOfWork.Repository<Brand, int>();

        // Markayı bul
        var brand = await brandRepo.AsQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Data.Id, cancellationToken);

        if (brand == null)
        {
            throw new KeyNotFoundException($"Marka bulunamadı: {request.Data.Id}");
        }

        // Güncelle
        brand.Code = request.Data.Code;
        brand.Name = request.Data.Name;
        brand.LogoUrl = request.Data.LogoUrl;
        brand.SortOrder = request.Data.SortOrder;
        brand.IsActive = request.Data.IsActive;
        brand.ProjectType = request.Data.ProjectType;

        brandRepo.Update(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // DTO'ya dönüştür ve dön
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
