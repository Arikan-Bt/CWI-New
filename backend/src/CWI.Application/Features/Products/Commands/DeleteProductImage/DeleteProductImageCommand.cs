using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace CWI.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommand : IRequest<bool>
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public DeleteProductImageCommandHandler(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var productImageRepo = _unitOfWork.Repository<ProductImage, int>();
        
        // Find the image record
        var productImage = await productImageRepo.AsQueryable()
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId && x.ImageUrl == request.ImageUrl, cancellationToken);

        if (productImage == null)
            return false;

        // Delete from database (Soft delete if ISoftDeletable, otherwise hard delete depends on Repository implementation)
        // AuditableEntity might handle it.
        productImageRepo.Delete(productImage);

        // Try to delete file from disk as well
        try
        {
            var wwwrootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(wwwrootPath, request.ImageUrl.Replace("/", "\\"));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Ignore file delete errors
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
