using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace CWI.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageCommand : IRequest<bool>
{
    public int ProductId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedFileExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public UploadProductImageCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<bool> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var productRepo = _unitOfWork.Repository<Product, int>();
        var product = await productRepo.AsQueryable()
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

        if (product == null)
            throw new InvalidOperationException("Product not found.");

        if (request.File == null || request.File.Length == 0)
            throw new InvalidOperationException("No file uploaded.");

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!AllowedFileExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Invalid file format. Allowed extensions: {string.Join(", ", AllowedFileExtensions)}");
        }

        var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "products");
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var dbPath = Path.Combine("uploads", "products", fileName).Replace("\\", "/");
        var fullPath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await request.File.CopyToAsync(stream, cancellationToken);

        // Add to ProductImage collection
        var productImageRepo = _unitOfWork.Repository<ProductImage, int>();
        var productImage = new ProductImage
        {
            ProductId = request.ProductId,
            ImageUrl = dbPath,
            IsActive = true,
            IsPrimary = product.Images.Count == 0,
            SortOrder = product.Images.Count + 1
        };

        await productImageRepo.AddAsync(productImage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
